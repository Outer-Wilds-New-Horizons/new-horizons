using NewHorizons.Builder.Body;
using NewHorizons.Builder.Props;
using NewHorizons.Builder.StarSystem;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props;
using NewHorizons.Handlers.TitleScreen;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

namespace NewHorizons.Handlers
{
    public static class TitleSceneHandler
    {
        internal static Dictionary<IModBehaviour, TitleScreenBuilderList> TitleScreenBuilders = new();
        internal static NewHorizonsBody[] eligibleBodies => Main.BodyDict.Values.ToList().SelectMany(x => x).ToList()
            .Where(b => (b.Config.HeightMap != null || b.Config.Atmosphere?.clouds != null) && b.Config.Star == null && b.Config.canShowOnTitle).ToArray();
        internal static int eligibleCount => eligibleBodies.Count();

        public static void Init()
        {
            var scene = SearchUtilities.Find("Scene");
            var background = SearchUtilities.Find("Scene/Background");
            var planetPivot = SearchUtilities.Find("Scene/Background/PlanetPivot");

            // Add fake sectors for ocean water component support
            scene.AddComponent<FakeSector>();
            background.AddComponent<FakeSector>();
            planetPivot.AddComponent<FakeSector>();

            // parent ambient light and campfire to the root (idk why they aren't parented in the first place mobius)
            var planetRoot = SearchUtilities.Find("Scene/Background/PlanetPivot/PlanetRoot");
            var campfire = SearchUtilities.Find("Scene/Background/PlanetPivot/Prefab_HEA_Campfire");
            campfire.transform.SetParent(planetRoot.transform, true);
            var ambientLight = SearchUtilities.Find("Scene/Background/PlanetPivot/AmbientLight_CaveTwin");
            ambientLight.transform.SetParent(planetRoot.transform, true);

            InitSubtitles();
            TitleScreenColourHandler.ResetColour(); // reset color at the start
            AudioTypeHandler.Init(); // init audio for custom music

            // Load player data for fact and persistent condition checking
            var profileManager = StandaloneProfileManager.SharedInstance;
            profileManager.PreInitialize();
            profileManager.Initialize();
            if (profileManager.currentProfile != null) // check if there is even a profile made yet
                PlayerData.Init(profileManager.currentProfileGameSave,
                    profileManager.currentProfileGameSettings,
                    profileManager.currentProfileGraphicsSettings,
                    profileManager.currentProfileInputJSON);

            // Grab configs and handlers and merge them into one list
            var validBuilders = TitleScreenBuilders.Values
                .Where(list => list.IsValid)
                .Select(list => list.GetRelevantBuilder()).ToList();

            var hasNHPlanets = eligibleCount != 0;

            // Get random index for the main builder
            var index = UnityEngine.Random.Range(0, validBuilders.Count());
            var randomBuilder = validBuilders.ElementAtOrDefault(index);
            if (randomBuilder != null)
            {
                validBuilders.RemoveAt(index);

                // display nh planets if not disabled
                if (!randomBuilder.DisableNHPlanets)
                {
                    DisplayBodiesOnTitleScreen();
                }

                // if it can share build extras
                if (randomBuilder.CanShare)
                {
                    // only build the ones that also can share and have the same value for disabling nh planet (if there is any nh planets)
                    foreach (var builder in validBuilders.Where(builder => builder.CanShare && (hasNHPlanets ? builder.DisableNHPlanets == randomBuilder.DisableNHPlanets : true)))
                    {
                        builder.Build();
                    }
                }
                
                // Build main one last so it overrides the extras
                randomBuilder.Build();
            }
            // default to displaying nh planets if no title screen builders
            else
            {
                DisplayBodiesOnTitleScreen();
            }

            try
            {
                Main.Instance.OnAllTitleScreensLoaded?.Invoke();
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Error in event handler for OnAllTitleScreensLoaded: {e}");
            }
        }

        public static void InitSubtitles()
        {
            GameObject subtitleContainer = SearchUtilities.Find("TitleMenu/TitleCanvas/TitleLayoutGroup/Logo_EchoesOfTheEye");
            
            if (subtitleContainer == null)
            {
                NHLogger.LogError("No subtitle container found! Failed to load subtitles.");
                return;
            }

            subtitleContainer.SetActive(true);
            subtitleContainer.AddComponent<SubtitlesHandler>();
        }

        public static void BuildConfig(IModBehaviour mod, TitleScreenConfig config)
        {
            if (config.menuTextTint != null)
            {
                TitleScreenColourHandler.SetColour(config.menuTextTint.ToColor());
            }

            if (config.Skybox?.destroyStarField ?? false)
            {
                UnityEngine.Object.Destroy(SearchUtilities.Find("Skybox/Starfield"));
            }

            if (config.Skybox?.rightPath != null ||
                config.Skybox?.leftPath != null ||
                config.Skybox?.topPath != null ||
                config.Skybox?.bottomPath != null ||
                config.Skybox?.frontPath != null ||
                config.Skybox?.bottomPath != null)
            {
                SkyboxBuilder.Make(config.Skybox, mod);
            }

            if (!string.IsNullOrEmpty(config.music))
            {
                var musicSource = SearchUtilities.Find("Scene/AudioSource_Music").GetComponent<OWAudioSource>();
                var audioType = AudioTypeHandler.GetAudioType(config.music, mod);
                Delay.FireOnNextUpdate(() => musicSource.AssignAudioLibraryClip(audioType));
            }

            if (!string.IsNullOrEmpty(config.ambience))
            {
                var ambienceSource = SearchUtilities.Find("Scene/AudioSource_Ambience").GetComponent<OWAudioSource>();
                var audioType = AudioTypeHandler.GetAudioType(config.ambience, mod);
                Delay.FireOnNextUpdate(() => ambienceSource.AssignAudioLibraryClip(audioType));
            }

            var background = SearchUtilities.Find("Scene/Background");
            var menuPlanet = SearchUtilities.Find("Scene/Background/PlanetPivot");

            if (config.Background != null)
            {
                if (config.Background.removeChildren != null)
                {
                    RemoveChildren(background, config.Background.removeChildren);
                }

                if (config.Background.details != null)
                {
                    foreach (var simplifiedDetail in config.Background.details)
                    {
                        DetailBuilder.Make(background, background.GetComponentInParent<Sector>(), mod, new DetailInfo(simplifiedDetail));
                    }
                }

                var rotator = background.GetComponent<RotateTransform>();
                rotator._degreesPerSecond = config.Background.rotationSpeed;
            }

            if (config.MenuPlanet != null)
            {
                if (config.MenuPlanet.removeChildren != null)
                {
                    RemoveChildren(menuPlanet, config.MenuPlanet.removeChildren);
                }

                if (config.MenuPlanet.details != null)
                {
                    foreach (var simplifiedDetail in config.MenuPlanet.details)
                    {
                        DetailBuilder.Make(menuPlanet, menuPlanet.GetComponentInParent<Sector>(), mod, new DetailInfo(simplifiedDetail));
                    }
                }

                var rotator = menuPlanet.GetComponent<RotateTransform>();
                rotator._localAxis = Vector3.up; // fix axis (because there is no reason for it to be negative when degrees were also negative)
                rotator._degreesPerSecond = config.MenuPlanet.rotationSpeed;

                if (config.MenuPlanet.destroyMenuPlanet)
                {
                    SearchUtilities.Find("Scene/Background/PlanetPivot/PlanetRoot").SetActive(false);
                }
            }
        }

        private static void RemoveChildren(GameObject go, string[] paths)
        {
            foreach (var childPath in paths)
            {
                var flag = true;
                foreach (var childObj in go.transform.FindAll(childPath))
                {
                    flag = false;
                    // idk why we wait here but we do
                    Delay.FireInNUpdates(() =>
                    {
                        if (childObj != null && childObj.gameObject != null)
                        {
                            childObj.gameObject.SetActive(false);
                        }
                    }, 2);
                }

                if (flag) NHLogger.LogWarning($"Couldn't find \"{childPath}\".");
            }
        }

        public static void DisplayBodiesOnTitleScreen()
        {
            try
            {
                // Try loading one planet why not
                var eligible = eligibleBodies;
                var eligibleCount = eligible.Count();
                if (eligibleCount == 0) return;

                var selectionCount = Mathf.Min(eligibleCount, 3);
                var indices = RandomUtility.GetUniqueRandomArray(0, eligible.Count(), selectionCount);

                NHLogger.LogVerbose($"Displaying {selectionCount} bodies on the title screen");

                var planetSizes = new List<(GameObject planet, float size)>();

                var bodyInfo = LoadTitleScreenBody(eligible[indices[0]]);
                bodyInfo.planet.transform.localRotation = Quaternion.Euler(15, 0, 0);
                planetSizes.Add(bodyInfo);

                if (selectionCount > 1)
                {
                    bodyInfo.planet.transform.localPosition = new Vector3(0, -15, 0);
                    bodyInfo.planet.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);

                    var bodyInfo2 = LoadTitleScreenBody(eligible[indices[1]]);
                    bodyInfo2.planet.transform.localPosition = new Vector3(7, 30, 0);
                    bodyInfo2.planet.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
                    planetSizes.Add(bodyInfo2);
                }

                if (selectionCount > 2)
                {
                    var bodyInfo3 = LoadTitleScreenBody(eligible[indices[2]]);
                    bodyInfo3.planet.transform.localPosition = new Vector3(-5, 10, 0);
                    bodyInfo3.planet.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
                    planetSizes.Add(bodyInfo3);
                }

                SearchUtilities.Find("Scene/Background/PlanetPivot/PlanetRoot").SetActive(false);

                var lightGO = new GameObject("Light");
                lightGO.transform.parent = SearchUtilities.Find("Scene/Background").transform;
                lightGO.transform.localPosition = new Vector3(-47.9203f, 145.7596f, 43.1802f);
                lightGO.transform.localRotation = Quaternion.Euler(13.1412f, 122.8785f, 169.4302f);
                var light = lightGO.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = Color.white;
                light.range = float.PositiveInfinity;
                light.intensity = 0.8f;

                // Resize planets relative to each other
                // If there are multiple planets shrink them down to 30% of the size
                var maxSize = planetSizes.Select(x => x.size).Max();
                var minSize = planetSizes.Select(x => x.size).Min();
                var multiplePlanets = planetSizes.Count > 1;
                foreach (var (planet, size) in planetSizes) 
                {
                    var adjustedSize = size / maxSize;
                    // If some planets would be too small we'll do a funny thing
                    if (minSize / maxSize < 0.3f)
                    {
                        var t = Mathf.InverseLerp(minSize, maxSize, size);
                        adjustedSize = Mathf.Lerp(0.3f, 1f, t * t);
                    }

                    planet.transform.localScale *= adjustedSize * (multiplePlanets ? 0.3f : 1f);
                }
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Failed to make title screen bodies: {e}");
            }
        }

        private static (GameObject planet, float size) LoadTitleScreenBody(NewHorizonsBody body)
        {
            NHLogger.LogVerbose($"Displaying {body.Config.name} on the title screen");
            var titleScreenGO = new GameObject(body.Config.name + "_TitleScreen");

            var maxRadius = -1f;

            if (body.Config.HeightMap != null)
            {
                HeightMapBuilder.Make(titleScreenGO, null, body.Config.HeightMap, body.Mod, 30);
                maxRadius = Mathf.Max(maxRadius, body.Config.HeightMap.maxHeight, body.Config.HeightMap.minHeight);
            }

            if (body.Config.Atmosphere?.clouds?.texturePath != null && body.Config.Atmosphere?.clouds?.cloudsPrefab != CloudPrefabType.Transparent)
            {
                // Hacky but whatever I just want a textured sphere
                var cloudTextureMap = new HeightMapModule();
                cloudTextureMap.maxHeight = cloudTextureMap.minHeight = body.Config.Atmosphere.size;
                cloudTextureMap.textureMap = body.Config.Atmosphere.clouds.texturePath;
                HeightMapBuilder.Make(titleScreenGO, null, cloudTextureMap, body.Mod, 30);

                maxRadius = Mathf.Max(maxRadius, cloudTextureMap.maxHeight);
            }

            if (body.Config.Base.groundSize > 0f)
            {
                MakeSphere(titleScreenGO, body.Config.Base.groundSize * 2f);
                maxRadius = Mathf.Max(maxRadius, body.Config.Base.groundSize);
            }

            if (body.Config.Water != null)
            {
                var waterRadius = MakeWater(body, titleScreenGO);
                maxRadius = Mathf.Max(maxRadius, waterRadius);
            }

            if (body.Config.Lava != null)
            {
                var lavaRadius = MakeLava(body, titleScreenGO);
                maxRadius = Mathf.Max(maxRadius, lavaRadius);
            }

            if (body.Config.Sand != null)
            {
                var sandRadius = MakeSand(body, titleScreenGO);
                maxRadius = Mathf.Max(maxRadius, sandRadius);
            }

            if (body.Config.Rings != null && body.Config.Rings.Length > 0)
            {
                foreach (var ring in body.Config.Rings)
                {
                    RingBuilder.Make(titleScreenGO, null, ring, body.Mod);

                    maxRadius = Mathf.Max(maxRadius, ring.outerRadius);
                }
            }

            var pivot = UnityEngine.Object.Instantiate(SearchUtilities.Find("Scene/Background/PlanetPivot"), SearchUtilities.Find("Scene/Background").transform);
            pivot.GetComponent<RotateTransform>()._degreesPerSecond = 10f;
            foreach (Transform child in pivot.transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            pivot.name = "Pivot";

            titleScreenGO.transform.parent = pivot.transform;
            titleScreenGO.transform.localPosition = Vector3.zero;
            titleScreenGO.transform.localScale = Vector3.one * 30f / maxRadius;

            return (titleScreenGO, maxRadius);
        }

        // Returns radius
        private static float MakeWater(NewHorizonsBody body, GameObject root)
        {
            var diameter = 2f * body.Config.Water.size * (body.Config.Water.curve?.FirstOrDefault()?.value ?? 1f);

            var mr = MakeSphere(root, diameter);
            var colour = body.Config.Water.tint?.ToColor() ?? Color.blue;
            mr.material.color = new Color(colour.r, colour.g, colour.b, 0.9f);

            // Make it transparent!
            mr.material.SetOverrideTag("RenderType", "Transparent");
            mr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mr.material.SetInt("_ZWrite", 0);
            mr.material.DisableKeyword("_ALPHATEST_ON");
            mr.material.DisableKeyword("_ALPHABLEND_ON");
            mr.material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mr.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            return diameter / 2f;
        }

        private static float MakeLava(NewHorizonsBody body, GameObject root)
        {
            var diameter = 2f * body.Config.Lava.size * (body.Config.Lava.curve?.FirstOrDefault()?.value ?? 1f);

            var mr = MakeSphere(root, diameter);
            mr.material.color = body.Config.Lava.tint?.ToColor() ?? Color.red;
            mr.material.SetColor("_EmissionColor", mr.material.color * 2f);

            return diameter / 2f;
        }

        private static float MakeSand(NewHorizonsBody body, GameObject root)
        {
            var diameter = 2f * body.Config.Sand.size * (body.Config.Sand.curve?.FirstOrDefault()?.value ?? 1f);

            var mr = MakeSphere(root, diameter);
            mr.material.color = body.Config.Sand.tint?.ToColor() ?? Color.yellow;
            mr.material.SetFloat("_Glossiness", 0);

            return diameter / 2f;
        }

        private static MeshRenderer MakeSphere(GameObject root, float diameter)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var meshRenderer = sphere.GetComponent<MeshRenderer>();
            sphere.transform.parent = root.transform;
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * diameter;

            return meshRenderer;
        }

        internal static void RegisterBuilder(IModBehaviour mod, ITitleScreenBuilder builder)
        {
            if (!TitleScreenBuilders.ContainsKey(mod))
                TitleScreenBuilders.Add(mod, new TitleScreenBuilderList());

            TitleScreenBuilders[mod].Add(builder);
        }

        public static void RegisterBuilder(IModBehaviour mod, TitleScreenConfig config)
            => RegisterBuilder(mod,
                new TitleScreenConfigBuilder(mod, config));

        public static void RegisterBuilder(IModBehaviour mod, Action<GameObject> builder, bool disableNHPlanets, bool shareTitleScreen, string persistentConditionRequired, string factRequired)
            => RegisterBuilder(mod,
                new TitleScreenBuilder(mod, builder,
                    disableNHPlanets, shareTitleScreen,
                    persistentConditionRequired, factRequired));

        internal class TitleScreenBuilderList
        {
            public List<ITitleScreenBuilder> list = new List<ITitleScreenBuilder>();

            public void Add(ITitleScreenBuilder builder)
            {
                list.Add(builder);
                builder.Index = list.IndexOf(builder);
            }

            public bool IsValid => GetRelevantBuilder() != null;

            public ITitleScreenBuilder GetRelevantBuilder()
            {
                return list.LastOrDefault(builder => builder.KnowsFact() && builder.HasCondition());
            }
        }

        internal class TitleScreenBuilder : ITitleScreenBuilder
        {
            public IModBehaviour mod;
            public Action<GameObject> builder;
            public bool disableNHPlanets;
            public bool shareTitleScreen;
            public string persistentConditionRequired;
            public string factRequired;

            public TitleScreenBuilder(IModBehaviour mod, Action<GameObject> builder, bool disableNHPlanets, bool shareTitleScreen, string persistentConditionRequired, string factRequired)
            {
                this.mod = mod;
                this.builder = builder;
                this.disableNHPlanets = disableNHPlanets;
                this.shareTitleScreen = shareTitleScreen;
                this.persistentConditionRequired = persistentConditionRequired;
                this.factRequired = factRequired;
            }

            public void Build()
            {
                NHLogger.LogVerbose($"Building handler {mod.ModHelper.Manifest.UniqueName} #{index}");
                try
                {
                    builder.Invoke(SearchUtilities.Find("Scene"));
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Error while building title screen handler {mod.ModHelper.Manifest.UniqueName} #{index}: {e}");
                }

                try
                {
                    Main.Instance.OnTitleScreenLoaded?.Invoke(mod.ModHelper.Manifest.UniqueName, index);
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Error in event handler for OnTitleScreenLoaded on title screen {mod.ModHelper.Manifest.UniqueName} #{index}: {e}");
                }
            }

            public IModBehaviour Mod => mod;

            public bool DisableNHPlanets => disableNHPlanets;

            public bool CanShare => shareTitleScreen;

            public bool KnowsFact() => string.IsNullOrEmpty(factRequired) || StandaloneProfileManager.SharedInstance.currentProfile != null && ShipLogHandler.KnowsFact(factRequired);

            public bool HasCondition() => string.IsNullOrEmpty(persistentConditionRequired) || StandaloneProfileManager.SharedInstance.currentProfile != null && PlayerData.GetPersistentCondition(persistentConditionRequired);

            private int index = -1;
            public int Index { get => index; set => index = value; }
        }

        internal class TitleScreenConfigBuilder : ITitleScreenBuilder
        {
            public IModBehaviour mod;
            public TitleScreenConfig config;

            public TitleScreenConfigBuilder(IModBehaviour mod, TitleScreenConfig config)
            {
                this.mod = mod;
                this.config = config;
            }

            public void Build()
            {
                NHLogger.LogVerbose($"Building config {mod.ModHelper.Manifest.UniqueName} #{index}");
                try
                {
                    BuildConfig(mod, config);
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Error while building title screen config {mod.ModHelper.Manifest.UniqueName} #{index}: {e}");
                }

                try
                {
                    Main.Instance.OnTitleScreenLoaded?.Invoke(mod.ModHelper.Manifest.UniqueName, index);
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Error in event handler for OnTitleScreenLoaded on title screen {mod.ModHelper.Manifest.UniqueName} #{index}: {e}");
                }
            }

            public IModBehaviour Mod => mod;

            public bool DisableNHPlanets => config.disableNHPlanets;

            public bool CanShare => config.shareTitleScreen;

            public bool KnowsFact() => string.IsNullOrEmpty(config.factRequiredForTitle) || StandaloneProfileManager.SharedInstance.currentProfile != null && ShipLogHandler.KnowsFact(config.factRequiredForTitle);

            public bool HasCondition() => string.IsNullOrEmpty(config.persistentConditionRequiredForTitle) || StandaloneProfileManager.SharedInstance.currentProfile != null && PlayerData.GetPersistentCondition(config.persistentConditionRequiredForTitle);

            private int index = -1;
            public int Index { get => index; set => index = value; }
        }

        internal interface ITitleScreenBuilder
        {
            IModBehaviour Mod { get; }

            bool DisableNHPlanets { get; }

            bool CanShare { get; }

            void Build();
            bool KnowsFact();
            bool HasCondition();

            int Index { get; set; }
        }

        /// <summary>
        /// For water and etc (they require a sector or else they will get deleted by detail builder)
        /// </summary>
        private class FakeSector : Sector
        {
            public override void Awake()
            {
                _triggerRoot = gameObject;
                _subsectors = new List<Sector>();
                _occupantMask = DynamicOccupant.Player;
                var parentSector = GetComponentsInParent<Sector>().FirstOrDefault(parentSector => parentSector != this);
                if (parentSector != null)
                {
                    _parentSector = parentSector;
                    _parentSector.AddSubsector(this);
                }
                SectorManager.RegisterSector(this);
            }

            public void Start()
            {
                OnSectorOccupantsUpdated.Invoke();
            }
        }
    }
}
