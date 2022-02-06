using NewHorizons.Atmosphere;
using NewHorizons.Body;
using NewHorizons.Builder.Body;
using NewHorizons.Builder.General;
using NewHorizons.Builder.Orbital;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.External.VariableSize;
using NewHorizons.OrbitalPhysics;
using NewHorizons.Utility;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Epic.OnlineServices;
using PacificEngine.OW_CommonResources.Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons
{
    public class Main : ModBehaviour
    {
        public static AssetBundle ShaderBundle;
        public static Main Instance { get; private set; }

        public static Dictionary<string, List<NewHorizonsBody>> BodyDict = new Dictionary<string, List<NewHorizonsBody>>();
        public static List<NewHorizonsBody> NextPassBodies = new List<NewHorizonsBody>();
        public static Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();
        public static float FurthestOrbit { get; set; } = 50000f;
        public StarLightController StarLightController { get; private set; }

        private string _currentStarSystem = "SolarSystem";
        public string CurrentStarSystem { get { return Instance._currentStarSystem; } }

        private bool _isChangingStarSystem = false;
        public bool IsWarping { get; private set; } = false;
        public bool WearingSuit { get; private set; } = false;

        public static bool HasWarpDrive { get; private set; } = false;

        private ShipWarpController _shipWarpController;

        public override object GetApi()
        {
            return new NewHorizonsApi();
        }

        public void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Instance = this;
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnDeath);
            ShaderBundle = Main.Instance.ModHelper.Assets.LoadBundle("AssetBundle/shader");
            BodyDict["SolarSystem"] = new List<NewHorizonsBody>();

            Tools.Patches.Apply();
            Tools.WarpDrivePatches.Apply();
            Tools.OWCameraFix.Apply();

            Logger.Log("Begin load of config files...", Logger.LogType.Log);

            try
            {
                LoadConfigs(this);
            }
            catch (Exception)
            {
                Logger.LogWarning("Couldn't find planets folder");
            }

            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single));
        }

        public void OnDestroy()
        {
            Logger.Log($"Destroying NewHorizons");
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnDeath);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.Log($"Scene Loaded: {scene.name} {mode}");

            _isChangingStarSystem = false;

            if (scene.name.Equals("TitleScreen")) DisplayBodyOnTitleScreen();

            if (scene.name != "SolarSystem")
            {
                // Reset back to original solar system after going to main menu.
                _currentStarSystem = "SolarSystem";
                return;
            }

            FurthestOrbit = 30000;

            HeavenlyBodyBuilder.Reset();

            NewHorizonsData.Load();

            // Make the warp controller if there are multiple star systems
            if (BodyDict.Keys.Count > 1)
            {
                HasWarpDrive = true;

                _shipWarpController = GameObject.Find("Ship_Body").AddComponent<ShipWarpController>();
                Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => ShipLogBuilder.Init());

                if (!PlayerData._currentGameSave.GetPersistentCondition("KnowsAboutWarpDrive"))
                {
                    LoadBody(LoadConfig(this, "AssetBundle/WarpDriveConfig.json"));    
                }
            }

            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => AstroObjectLocator.GetAstroObject("MapSatellite").gameObject.AddComponent<MapSatelliteOrbitFix>());

            // Need to manage this when there are multiple stars
            var sun = GameObject.Find("Sun_Body");
            var starController = sun.AddComponent<StarController>();
            starController.Light = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>();
            starController.AmbientLight = GameObject.Find("Sun_Body/AmbientLight_SUN").GetComponent<Light>();
            starController.FaceActiveCamera = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<FaceActiveCamera>();
            starController.CSMTextureCacher = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<CSMTextureCacher>();
            starController.ProxyShadowLight = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<ProxyShadowLight>();
            starController.Intensity = 0.9859f;
            starController.SunColor = new Color(1f, 0.8845f, 0.6677f, 1f);

            var starLightGO = GameObject.Instantiate(sun.GetComponentInChildren<SunLightController>().gameObject);
            foreach(var comp in starLightGO.GetComponents<Component>())
            {
                if(!(comp is SunLightController) && !(comp is SunLightParamUpdater) && !(comp is Light) && !(comp is Transform))
                {
                    GameObject.Destroy(comp);
                }
            }
            GameObject.Destroy(starLightGO.GetComponent<Light>());
            starLightGO.name = "StarLightController";

            StarLightController = starLightGO.AddComponent<StarLightController>();
            StarLightController.AddStar(starController);
                                                                                                                   
            starLightGO.SetActive(true);

            // TODO: Make this configurable probably
            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => Locator.GetPlayerBody().gameObject.AddComponent<DebugRaycaster>());

            // Some builders need to be reset each time
            SignalBuilder.Reset();

            // We do our own AstroObject tracking
            AstroObjectLocator.RefreshList();
            foreach (AstroObject ao in GameObject.FindObjectsOfType<AstroObject>())
            {
                AstroObjectLocator.AddAstroObject(ao);
            }

            // Order by stars then planets then moons (not necessary but probably speeds things up, maybe) ALSO only include current star system
            var toLoad = BodyDict[_currentStarSystem]
                .OrderBy(b =>
                (b.Config.BuildPriority != -1 ? b.Config.BuildPriority :
                (b.Config.FocalPoint != null ? 0 :
                (b.Config.Star != null) ? 0 :
                (b.Config.Orbit.IsMoon ? 2 : 1)
                ))).ToList();

            var passCount = 0;
            while (toLoad.Count != 0)
            {
                Logger.Log($"Starting body loading pass #{++passCount}");
                var flagNoneLoadedThisPass = true;
                foreach (var body in toLoad)
                {
                    if (LoadBody(body)) flagNoneLoadedThisPass = false;
                }
                if (flagNoneLoadedThisPass)
                {
                    Logger.LogWarning("No objects were loaded this pass");
                    // Try again but default to sun
                    foreach (var body in toLoad)
                    {
                        if (LoadBody(body, true)) flagNoneLoadedThisPass = false;
                    }
                }
                if (flagNoneLoadedThisPass)
                {
                    // Give up
                    Logger.Log($"Couldn't finish adding bodies.");
                    return;
                }

                toLoad = NextPassBodies;
                NextPassBodies = new List<NewHorizonsBody>();

                // Infinite loop failsafe
                if (passCount > 10)
                {
                    Logger.Log("Something went wrong");
                    break;
                }
            }

            Logger.Log("Done loading bodies");

            // I don't know what these do but they look really weird from a distance
            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => PlanetDestroyer.RemoveDistantProxyClones());

            if(!_currentStarSystem.Equals("SolarSystem")) Instance.ModHelper.Events.Unity.FireInNUpdates(() => PlanetDestroyer.RemoveSolarSystem(), 2);

            var map = GameObject.FindObjectOfType<MapController>();
            if (map != null) map._maxPanDistance = FurthestOrbit * 1.5f;

            Logger.Log($"Is the player warping in? {IsWarping}");
            if (IsWarping && _shipWarpController) Instance.ModHelper.Events.Unity.FireInNUpdates(() => _shipWarpController.WarpIn(WearingSuit), 1);
            IsWarping = false;
        }

        #region TitleScreen

        public void DisplayBodyOnTitleScreen()
        {
            //Try loading one planet why not
            var eligible = BodyDict.Values.SelectMany(x => x).ToList().Where(b => (b.Config.HeightMap != null || b.Config.Atmosphere?.Cloud != null) && b.Config.Star == null).ToArray();
            var eligibleCount = eligible.Count();
            if (eligibleCount == 0) return;

            var selectionCount = Mathf.Min(eligibleCount, 3);
            var indices = RandomUtility.GetUniqueRandomArray(0, eligible.Count(), selectionCount);

            Logger.Log($"Displaying {selectionCount} bodies on the title screen");

            GameObject body1, body2, body3;

            body1 = LoadTitleScreenBody(eligible[indices[0]]);
            body1.transform.localRotation = Quaternion.Euler(15, 0, 0);
            if (selectionCount > 1)
            {
                body1.transform.localScale = Vector3.one * (body1.transform.localScale.x) * 0.3f;
                body1.transform.localPosition = new Vector3(0, -15, 0);
                body1.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
                body2 = LoadTitleScreenBody(eligible[indices[1]]);
                body2.transform.localScale = Vector3.one * (body2.transform.localScale.x) * 0.3f;
                body2.transform.localPosition = new Vector3(7, 30, 0);
                body2.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            }
            if (selectionCount > 2)
            {
                body3 = LoadTitleScreenBody(eligible[indices[2]]);
                body3.transform.localScale = Vector3.one * (body3.transform.localScale.x) * 0.3f;
                body3.transform.localPosition = new Vector3(-5, 10, 0);
                body3.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            }

            GameObject.Find("Scene/Background/PlanetPivot/Prefab_HEA_Campfire").SetActive(false);
            GameObject.Find("Scene/Background/PlanetPivot/PlanetRoot").SetActive(false);

            var lightGO = new GameObject("Light");
            lightGO.transform.parent = GameObject.Find("Scene/Background").transform;
            lightGO.transform.localPosition = new Vector3(-47.9203f, 145.7596f, 43.1802f);
            var light = lightGO.AddComponent<Light>();
            light.color = new Color(1f, 1f, 1f, 1f);
            light.range = 100;
            light.intensity = 0.8f;
        }

        private GameObject LoadTitleScreenBody(NewHorizonsBody body)
        {
            Logger.Log($"Displaying {body.Config.Name} on the title screen");
            GameObject titleScreenGO = new GameObject(body.Config.Name + "_TitleScreen");
            HeightMapModule heightMap = new HeightMapModule();
            var minSize = 15;
            var maxSize = 30;
            float size = minSize;
            if (body.Config.HeightMap != null)
            {
                size = Mathf.Clamp(body.Config.HeightMap.MaxHeight / 10, minSize, maxSize);
                heightMap.TextureMap = body.Config.HeightMap.TextureMap;
                heightMap.HeightMap = body.Config.HeightMap.HeightMap;
                heightMap.MaxHeight = size;
                heightMap.MinHeight = body.Config.HeightMap.MinHeight * size / body.Config.HeightMap.MaxHeight;
            }
            if (body.Config.Atmosphere != null && body.Config.Atmosphere.Cloud != null)
            {
                // Hacky but whatever I just want a sphere
                size = Mathf.Clamp(body.Config.Atmosphere.Size / 10, minSize, maxSize);
                heightMap.MaxHeight = heightMap.MinHeight = size + 1;
                heightMap.TextureMap = body.Config.Atmosphere.Cloud;
            }

            HeightMapBuilder.Make(titleScreenGO, heightMap, body.Mod.Assets);

            GameObject pivot = GameObject.Instantiate(GameObject.Find("Scene/Background/PlanetPivot"), GameObject.Find("Scene/Background").transform);
            pivot.GetComponent<RotateTransform>()._degreesPerSecond = 10f;
            foreach (Transform child in pivot.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            pivot.name = "Pivot";

            if (body.Config.Ring != null)
            {
                RingModule newRing = new RingModule();
                newRing.InnerRadius = size * 1.2f;
                newRing.OuterRadius = size * 2f;
                newRing.Texture = body.Config.Ring.Texture;
                var ring = RingBuilder.Make(titleScreenGO, newRing, body.Mod.Assets);
                titleScreenGO.transform.localScale = Vector3.one * 0.8f;
            }

            titleScreenGO.transform.parent = pivot.transform;
            titleScreenGO.transform.localPosition = Vector3.zero;

            return titleScreenGO;
        }

        #endregion TitleScreen

        #region Load
        public void LoadConfigs(IModBehaviour mod)
        {
            var folder = mod.ModHelper.Manifest.ModFolderPath;
            foreach (var file in Directory.GetFiles(folder + @"planets\", "*.json", SearchOption.AllDirectories))
            {
                var relativeDirectory = file.Replace(folder, "");
                var body = LoadConfig(mod, relativeDirectory);

                if(body != null)
                {
                    BodyDict[body.Config.StarSystem].Add(body);
                }
            }
        }

        public NewHorizonsBody LoadConfig(IModBehaviour mod, string relativeDirectory)
        {
            NewHorizonsBody body = null;
            try
            {
                var config = mod.ModHelper.Storage.Load<PlanetConfig>(relativeDirectory);
                Logger.Log($"Loaded {config.Name}");
                if (config.Base.CenterOfSolarSystem) config.Orbit.IsStatic = true;
                if (!BodyDict.ContainsKey(config.StarSystem)) BodyDict.Add(config.StarSystem, new List<NewHorizonsBody>());

                body = new NewHorizonsBody(config, mod.ModHelper);
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load {relativeDirectory}: {e.Message}, is your Json formatted correctly?");
            }

            return body;
        }

        private bool LoadBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
        {
            // I don't remember doing this why is it exceptions what am I doing
            GameObject existingPlanet = null;
            try
            {
                existingPlanet = AstroObjectLocator.GetAstroObject(body.Config.Name).gameObject;
            }
            catch (Exception)
            {
                existingPlanet = GameObject.Find(body.Config.Name.Replace(" ", "") + "_Body");
            }

            if (existingPlanet != null)
            {
                try
                {
                    if (body.Config.Destroy)
                    {
                        var ao = existingPlanet.GetComponent<AstroObject>();
                        if (ao != null) Instance.ModHelper.Events.Unity.FireInNUpdates(() => PlanetDestroyer.RemoveBody(ao), 5);
                        else Instance.ModHelper.Events.Unity.FireInNUpdates(() => existingPlanet.SetActive(false), 5);
                    }
                    else UpdateBody(body, existingPlanet);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't update body {body.Config?.Name}: {e.Message}, {e.StackTrace}");
                    return false;
                }
            }
            else
            {
                try
                {
                    GameObject planetObject = GenerateBody(body, defaultPrimaryToSun);
                    if (planetObject == null) return false;
                    planetObject.SetActive(true);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't generate body {body.Config?.Name}: {e.Message}, {e.StackTrace}");
                    return false;
                }
            }
            return true;
        }

        public GameObject UpdateBody(NewHorizonsBody body, GameObject go)
        {
            Logger.Log($"Updating existing Object {go.name}");

            var sector = go.GetComponentInChildren<Sector>();
            var rb = go.GetAttachedOWRigidbody();

            if (body.Config.ChildrenToDestroy != null && body.Config.ChildrenToDestroy.Length > 0)
            {
                foreach (var child in body.Config.ChildrenToDestroy)
                {
                    Instance.ModHelper.Events.Unity.FireInNUpdates(() => GameObject.Find(go.name + "/" + child).SetActive(false), 2);
                }
            }

            // Do stuff that's shared between generating new planets and updating old ones
            return SharedGenerateBody(body, go, sector, rb);
        }

        #endregion Load

        #region Body generation

        public GameObject GenerateBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
        {
            AstroObject primaryBody;
            if (body.Config.Orbit.PrimaryBody != null)
            {
                primaryBody = AstroObjectLocator.GetAstroObject(body.Config.Orbit.PrimaryBody);
                if (primaryBody == null)
                {
                    if (defaultPrimaryToSun)
                    {
                        Logger.Log($"Couldn't find {body.Config.Orbit.PrimaryBody}, defaulting to Sun");
                        primaryBody = AstroObjectLocator.GetAstroObject("Sun");
                    }
                    else
                    {
                        NextPassBodies.Add(body);
                        return null;
                    }
                }
            }
            else
            {
                primaryBody = null;
            }

            Logger.Log($"Begin generation sequence of [{body.Config.Name}]");

            var go = new GameObject(body.Config.Name.Replace(" ", "").Replace("'", "") + "_Body");
            go.SetActive(false);

            if (body.Config.Base.GroundSize != 0) GeometryBuilder.Make(go, body.Config.Base.GroundSize);

            var atmoSize = body.Config.Atmosphere != null ? body.Config.Atmosphere.Size : 0f;
            float sphereOfInfluence = Mathf.Max(Mathf.Max(atmoSize, 50), body.Config.Base.SurfaceSize * 2f);
            var overrideSOI = body.Config.Base.SphereOfInfluence;
            if (overrideSOI != 0) sphereOfInfluence = overrideSOI;

            var outputTuple = BaseBuilder.Make(go, primaryBody, body.Config);
            var ao = (AstroObject)outputTuple.Item1;
            var owRigidBody = (OWRigidbody)outputTuple.Item2;

            GravityVolume gv = null;
            if (body.Config.Base.SurfaceGravity != 0)
                gv = GravityBuilder.Make(go, ao, body.Config);

            if (body.Config.Base.HasReferenceFrame)
                RFVolumeBuilder.Make(go, owRigidBody, sphereOfInfluence);

            if (body.Config.Base.HasMapMarker)
                MarkerBuilder.Make(go, body.Config.Name, body.Config);

            if (body.Config.Base.HasAmbientLight)
                AmbientLightBuilder.Make(go, sphereOfInfluence);

            var sector = MakeSector.Make(go, owRigidBody, sphereOfInfluence * 2f);
            ao.SetValue("_rootSector", sector);

            VolumesBuilder.Make(go, body.Config.Base.SurfaceSize, sphereOfInfluence);

            if (body.Config.HeightMap != null)
                HeightMapBuilder.Make(go, body.Config.HeightMap, body.Mod.Assets);

            if (body.Config.ProcGen != null)
                ProcGenBuilder.Make(go, body.Config.ProcGen);

            if (body.Config.Star != null) StarLightController.AddStar(StarBuilder.Make(go, sector, body.Config.Star));

            if (body.Config.FocalPoint != null)
                FocalPointBuilder.Make(go, body.Config.FocalPoint);

            // Do stuff that's shared between generating new planets and updating old ones
            go = SharedGenerateBody(body, go, sector, owRigidBody);

            body.Object = go;

            // Now that we're done move the planet into place
            go.transform.parent = Locator.GetRootTransform();
            go.transform.position = OrbitalHelper.GetCartesian(new OrbitalHelper.Gravity(1, 100), body.Config.Orbit).Item1 + (primaryBody == null ? Vector3.zero : primaryBody.transform.position);

            if (go.transform.position.magnitude > FurthestOrbit)
            {
                FurthestOrbit = go.transform.position.magnitude + 30000f;
            }

            // Have to do this after setting position
            var initialMotion = InitialMotionBuilder.Make(go, primaryBody, owRigidBody, body.Config.Orbit);

            // Spawning on other planets is a bit hacky so we do it last
            if (body.Config.Spawn != null)
            {
                SpawnPointBuilder.Make(go, body.Config.Spawn, owRigidBody);
            }

            if (body.Config.Orbit.ShowOrbitLine && !body.Config.Orbit.IsStatic) OrbitlineBuilder.Make(body.Object, ao, body.Config.Orbit.IsMoon, body.Config);

            if (!body.Config.Orbit.IsStatic) DetectorBuilder.Make(go, owRigidBody, primaryBody, ao);

            if (ao.GetAstroObjectName() == AstroObject.Name.CustomString) AstroObjectLocator.RegisterCustomAstroObject(ao);

            HeavenlyBodyBuilder.Make(go, body.Config, sphereOfInfluence, gv, initialMotion);

            return go;
        }

        private GameObject SharedGenerateBody(NewHorizonsBody body, GameObject go, Sector sector, OWRigidbody rb)
        {
            if (body.Config.Ring != null)
                RingBuilder.Make(go, body.Config.Ring, body.Mod.Assets);

            if (body.Config.AsteroidBelt != null)
                AsteroidBeltBuilder.Make(body.Config.Name, body.Config, body.Mod);

            if (body.Config.Base.HasCometTail)
                CometTailBuilder.Make(go, body.Config.Base, go.GetComponent<AstroObject>().GetPrimaryBody());

            // Backwards compatability
            if (body.Config.Base.LavaSize != 0)
            {
                var lava = new LavaModule();
                lava.Size = body.Config.Base.LavaSize;
                LavaBuilder.Make(go, sector, rb, lava);
            }

            if (body.Config.Lava != null)                                                                                     
                LavaBuilder.Make(go, sector, rb, body.Config.Lava);

            // Backwards compatability
            if (body.Config.Base.WaterSize != 0)
            {
                var water = new WaterModule();
                water.Size = body.Config.Base.WaterSize;
                water.Tint = body.Config.Base.WaterTint;
                WaterBuilder.Make(go, sector, rb, water);
            }

            if (body.Config.Water != null)
                WaterBuilder.Make(go, sector, rb, body.Config.Water);

            if (body.Config.Sand != null)
                SandBuilder.Make(go, sector, rb, body.Config.Sand);

            if (body.Config.Atmosphere != null)
            {
                AirBuilder.Make(go, body.Config.Atmosphere.Size, body.Config.Atmosphere.HasRain, body.Config.Atmosphere.HasOxygen);

                if (body.Config.Atmosphere.Cloud != null)
                {
                    CloudsBuilder.Make(go, sector, body.Config.Atmosphere, body.Mod.Assets);
                    SunOverrideBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere);
                }

                if (body.Config.Atmosphere.HasRain || body.Config.Atmosphere.HasSnow)
                    EffectsBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere.Size / 2f, body.Config.Atmosphere.HasRain, body.Config.Atmosphere.HasSnow);

                if (body.Config.Atmosphere.FogSize != 0)
                    FogBuilder.Make(go, sector, body.Config.Atmosphere);

                AtmosphereBuilder.Make(go, body.Config.Atmosphere, body.Config.Base.SurfaceSize);
            }

            if (body.Config.Props != null)
                PropBuildManager.Make(go, sector, body.Config, body.Mod, body.Mod.Manifest.UniqueName);

            if (body.Config.Signal != null)
                SignalBuilder.Make(go, sector, body.Config.Signal, body.Mod);

            if (body.Config.Base.BlackHoleSize != 0 || body.Config.Singularity != null)
                SingularityBuilder.Make(go, sector, rb, body.Config);

            if (body.Config.Funnel != null)
                FunnelBuilder.Make(go, go.GetComponentInChildren<ConstantForceDetector>(), rb, body.Config.Funnel);

            return go;
        }

        #endregion Body generation

        #region Change star system
        public void ChangeCurrentStarSystem(string newStarSystem, bool warp = false)
        {
            if (_isChangingStarSystem) return;

            Logger.Log($"Warping to {newStarSystem}");
            if(warp && _shipWarpController) _shipWarpController.WarpOut();
            _currentStarSystem = newStarSystem;
            _isChangingStarSystem = true;
            IsWarping = warp;
            WearingSuit = PlayerState.IsWearingSuit();

            // We kill them so they don't move as much
            Locator.GetDeathManager().KillPlayer(DeathType.Meditation);

            LoadManager.LoadSceneAsync(OWScene.SolarSystem, true, LoadManager.FadeType.ToBlack, 0.1f, true);
        }

        void OnDeath(DeathType _)
        {
            // We reset the solar system on death (unless we just killed the player)
            if (!_isChangingStarSystem) _currentStarSystem = "SolarSystem";
        }
        #endregion Change star system
    }

    #region API
    public class NewHorizonsApi
    {
        [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use Create(Dictionary<string, object> config, IModBehaviour mod) instead")]
        public void Create(Dictionary<string, object> config)
        {
            Create(config, null);
        }

        public void Create(Dictionary<string, object> config, IModBehaviour mod)
        {
            Logger.Log("Recieved API request to create planet " + (string)config["Name"], Logger.LogType.Log);
            var planetConfig = new PlanetConfig(config);

            var body = new NewHorizonsBody(planetConfig, mod != null ? mod.ModHelper : Main.Instance.ModHelper);

            if (!Main.BodyDict.ContainsKey(body.Config.StarSystem)) Main.BodyDict.Add(body.Config.StarSystem, new List<NewHorizonsBody>());
            Main.BodyDict[body.Config.StarSystem].Add(body);
        }

        public void LoadConfigs(IModBehaviour mod)
        {
            Main.Instance.LoadConfigs(mod);
        }

        public GameObject GetPlanet(string name)
        {
            return Main.BodyDict.Values.SelectMany(x => x).ToList().FirstOrDefault(x => x.Config.Name == name).Object;
        }
    }
    #endregion API
}
