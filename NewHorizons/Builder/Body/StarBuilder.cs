using NewHorizons.Components.SizeControllers;
using NewHorizons.Components.Stars;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using System.Linq;
using UnityEngine;
using static Mono.Security.X509.X520;

namespace NewHorizons.Builder.Body
{
    public static class StarBuilder
    {
        public const float OuterRadiusRatio = 1.5f;
        private static Texture2D _colorOverTime;
        private static Texture2D _supernovaEffects;
        private static readonly int ColorRamp = Shader.PropertyToID("_ColorRamp");
        private static readonly int SkyColor = Shader.PropertyToID("_SkyColor");
        private static readonly int Tint = Shader.PropertyToID("_Tint");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");
        private static readonly int OuterRadius = Shader.PropertyToID("_OuterRadius");

        private static GameObject _starAudio;
        private static GameObject _starAtmosphere;
        private static GameObject _starProxyAtmosphere;
        private static GameObject _starAmbientLight;
        private static GameObject _sunLight;
        private static GameObject _starSurface;
        private static GameObject _starSolarFlareEmitter;
        private static GameObject _supernovaPrefab;
        private static Material _mainSequenceMaterial;
        private static Material _giantMaterial;
        private static Material _flareMaterial;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_colorOverTime == null) _colorOverTime = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/StarColorOverTime.png");
            if (_supernovaEffects == null) _supernovaEffects = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/Effects_SUN_Supernova_d.png");

            if (_isInit) return;

            _isInit = true;

            if (_starAudio == null) _starAudio = SearchUtilities.Find("Sun_Body/Sector_SUN/Audio_SUN").InstantiateInactive().Rename("Prefab_Audio_Star").DontDestroyOnLoad();
            if (_starAtmosphere == null) _starAtmosphere = SearchUtilities.Find("Sun_Body/Atmosphere_SUN").InstantiateInactive().Rename("Prefab_Atmosphere_Star").DontDestroyOnLoad();
            if (_starAmbientLight == null) _starAmbientLight = SearchUtilities.Find("Sun_Body/AmbientLight_SUN").InstantiateInactive().Rename("Prefab_AmbientLight_Star").DontDestroyOnLoad();
            if (_sunLight == null) _sunLight = SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").InstantiateInactive().Rename("Prefab_SunLight").DontDestroyOnLoad();
            if (_starProxyAtmosphere == null) _starProxyAtmosphere = Object.FindObjectOfType<DistantProxyManager>()._sunProxyPrefab.FindChild("Sun_Proxy_Body/Atmosphere_SUN").InstantiateInactive().Rename("Prefab_ProxyAtmosphere_Star").DontDestroyOnLoad();
            if (_starSurface == null) _starSurface = SearchUtilities.Find("Sun_Body/Sector_SUN/Geometry_SUN/Surface").InstantiateInactive().Rename("Prefab_Surface_Star").DontDestroyOnLoad();
            if (_starSolarFlareEmitter == null) _starSolarFlareEmitter = SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SolarFlareEmitter").InstantiateInactive().Rename("Prefab_SolarFlareEmitter_Star").DontDestroyOnLoad();
            if (_supernovaPrefab == null) _supernovaPrefab = SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/Supernova").InstantiateInactive().Rename("Prefab_Supernova").DontDestroyOnLoad();
            
            if (_mainSequenceMaterial == null) _mainSequenceMaterial = new Material(SearchUtilities.Find("Sun_Body").GetComponent<SunController>()._startSurfaceMaterial).DontDestroyOnLoad();
            if (_giantMaterial == null) _giantMaterial = new Material(SearchUtilities.Find("Sun_Body").GetComponent<SunController>()._endSurfaceMaterial).DontDestroyOnLoad();
            if (_flareMaterial == null)
            {
                _flareMaterial = new Material(_starSolarFlareEmitter.GetComponentInChildren<SolarFlareController>().GetComponent<MeshRenderer>().sharedMaterial).DontDestroyOnLoad();
                _flareMaterial.SetColor(Shader.PropertyToID("_Color"), Color.white);
            }
        }

        public static (GameObject, StarController, StarEvolutionController, Light) Make(GameObject planetGO, Sector sector, StarModule starModule, IModBehaviour mod, bool isStellarRemnant)
        {
            InitPrefabs();

            var (starGO, starEvolutionController, supernova) = SharedStarGeneration(planetGO, sector, mod, starModule, isStellarRemnant);

            var sunAudio = Object.Instantiate(_starAudio, starGO.transform);
            sunAudio.transform.localPosition = Vector3.zero;
            sunAudio.transform.localScale = Vector3.one;
            sunAudio.transform.Find("SurfaceAudio_Sun").GetComponent<AudioSource>().maxDistance = starModule.size * 2f;
            var sunSurfaceAudio = sunAudio.GetComponentInChildren<SunSurfaceAudioController>();
            var surfaceAudio = sunSurfaceAudio.gameObject.AddComponent<StarSurfaceAudioController>();
            surfaceAudio._size = starModule.size;
            Object.Destroy(sunSurfaceAudio);
            surfaceAudio.SetSector(sector);

            sunAudio.name = "Audio_Star";
            sunAudio.SetActive(true);

            GameObject sunAtmosphere = null;
            if (starModule.hasAtmosphere)
            {
                sunAtmosphere = Object.Instantiate(_starAtmosphere, starGO.transform);
                sunAtmosphere.transform.position = planetGO.transform.position;
                sunAtmosphere.transform.localScale = Vector3.one * OuterRadiusRatio;
                sunAtmosphere.name = "Atmosphere_Star";
                sunAtmosphere.SetActive(true);

                var atmospheres = sunAtmosphere.transform.Find("AtmoSphere");
                atmospheres.transform.localScale = Vector3.one;
                var lods = atmospheres.GetComponentsInChildren<MeshRenderer>();
                foreach (var lod in lods)
                {
                    lod.material.SetFloat(InnerRadius, starModule.size);
                    lod.material.SetFloat(OuterRadius, starModule.size * OuterRadiusRatio);
                }

                var fog = sunAtmosphere.transform.Find("FogSphere").GetComponent<PlanetaryFogController>();
                fog.transform.localScale = Vector3.one;
                fog.fogRadius = starModule.size * OuterRadiusRatio;
                fog.lodFadeDistance = fog.fogRadius * (OuterRadiusRatio - 1f);

                fog.fogImpostor.material.SetFloat(Radius, starModule.size * OuterRadiusRatio);
                if (starModule.tint != null)
                {
                    fog.fogTint = starModule.tint.ToColor();
                    fog.fogImpostor.material.SetColor(Tint, starModule.tint.ToColor());
                    foreach (var lod in lods)
                        lod.material.SetColor(SkyColor, starModule.tint.ToColor());
                }
            }

            var ambientLightGO = Object.Instantiate(_starAmbientLight, starGO.transform);
            ambientLightGO.transform.localPosition = Vector3.zero;
            ambientLightGO.name = "AmbientLight_Star";
            ambientLightGO.SetActive(true);

            Light ambientLight = ambientLightGO.GetComponent<Light>();
            ambientLight.range = starModule.size * OuterRadiusRatio;

            var heatVolume = new GameObject("HeatVolume");
            heatVolume.transform.SetParent(starGO.transform, false);
            heatVolume.transform.localPosition = Vector3.zero;
            heatVolume.transform.localScale = Vector3.one;
            heatVolume.layer = Layer.BasicEffectVolume;
            heatVolume.AddComponent<SphereShape>().radius = 1.1f;
            heatVolume.AddComponent<OWTriggerVolume>();
            heatVolume.AddComponent<HeatHazardVolume>()._damagePerSecond = 20f;

            // Kill player entering the star
            var deathVolume = new GameObject("DestructionFluidVolume");
            deathVolume.transform.SetParent(starGO.transform, false);
            deathVolume.transform.localPosition = Vector3.zero;
            deathVolume.transform.localScale = Vector3.one;
            deathVolume.layer = Layer.BasicEffectVolume;
            var sphereCollider = deathVolume.AddComponent<SphereCollider>();
            sphereCollider.radius = 1f;
            sphereCollider.isTrigger = true;
            deathVolume.AddComponent<OWCollider>();
            deathVolume.AddComponent<OWTriggerVolume>();
            var destructionVolume = deathVolume.AddComponent<DestructionVolume>();
            destructionVolume._onlyAffectsPlayerAndShip = true;
            destructionVolume._deathType = DeathType.Energy;
            var starFluidVolume = deathVolume.AddComponent<StarFluidVolume>();

            // Destroy planets entering the star
            var planetDestructionVolume = new GameObject("PlanetDestructionVolume");
            planetDestructionVolume.transform.SetParent(starGO.transform, false);
            planetDestructionVolume.transform.localPosition = Vector3.zero;
            planetDestructionVolume.transform.localScale = Vector3.one;
            planetDestructionVolume.layer = Layer.BasicEffectVolume;
            var planetSphereCollider = planetDestructionVolume.AddComponent<SphereCollider>();
            planetSphereCollider.radius = 0.8f;
            planetSphereCollider.isTrigger = true;
            planetDestructionVolume.AddComponent<OWCollider>();
            planetDestructionVolume.AddComponent<OWTriggerVolume>();
            planetDestructionVolume.AddComponent<StarDestructionVolume>()._deathType = DeathType.Energy;

            // Star light
            var sunLight = new GameObject("StarLight");
            sunLight.transform.parent = starGO.transform;
            sunLight.transform.localPosition = Vector3.zero;
            sunLight.transform.localScale = Vector3.one;

            var light = sunLight.AddComponent<Light>();
            light.CopyPropertiesFrom(_sunLight.GetComponent<Light>());
            light.intensity *= starModule.solarLuminosity;
            light.range = starModule.lightRadius;

            Color lightColour = light.color;
            if (starModule.lightTint != null) lightColour = starModule.lightTint.ToColor();

            light.color = lightColour;
            ambientLight.color = new Color(lightColour.r, lightColour.g, lightColour.b, lightColour.a == 0 ? 0.0001f : lightColour.a);

            // used to use CopyPropertiesFrom, but that doesnt work here. instead, just copy settings from unity explorer
            var faceActiveCamera = sunLight.AddComponent<FaceActiveCamera>();
            faceActiveCamera._useLookAt = true;
            var csmTextureCacher = sunLight.AddComponent<CSMTextureCacher>();
            var proxyShadowLight = sunLight.AddComponent<ProxyShadowLight>();

            sunLight.name = "StarLight";

            // Star controller (works on atmospheric shaders)
            StarController starController = null;
            if (starModule.solarLuminosity != 0 && starModule.hasStarController)
            {
                starController = planetGO.AddComponent<StarController>();
                starController.Light = light;
                starController.AmbientLight = ambientLight;
                starController.FaceActiveCamera = faceActiveCamera;
                starController.CSMTextureCacher = csmTextureCacher;
                starController.ProxyShadowLight = proxyShadowLight;
                starController.Intensity = starModule.solarLuminosity;
                starController.SunColor = lightColour;
                starController.IsStellarRemnant = isStellarRemnant;
            }

            starEvolutionController.atmosphere = sunAtmosphere;
            starEvolutionController.controller = starController;
            starEvolutionController.light = light;

            starEvolutionController.supernovaColour = starModule.supernovaTint;
            starFluidVolume.SetStarEvolutionController(starEvolutionController);

            surfaceAudio.SetStarEvolutionController(starEvolutionController);

            starEvolutionController.heatVolume = heatVolume.GetComponent<HeatHazardVolume>();
            starEvolutionController.destructionVolume = deathVolume.GetComponent<DestructionVolume>();
            starEvolutionController.planetDestructionVolume = planetDestructionVolume.GetComponent<StarDestructionVolume>();
            starEvolutionController.starFluidVolume = starFluidVolume;
            starEvolutionController.oneShotSource = sunAudio.transform.Find("OneShotAudio_Sun")?.GetComponent<OWAudioSource>();

            var shockLayerRuleset = sector.gameObject.AddComponent<ShockLayerRuleset>();
            shockLayerRuleset._type = ShockLayerRuleset.ShockType.Radial;
            shockLayerRuleset._trailLength = 50;
            shockLayerRuleset._radialCenter = deathVolume.transform;
            shockLayerRuleset._innerRadius = 0;
            shockLayerRuleset._outerRadius = starModule.size * OuterRadiusRatio;
            if (starModule.tint != null) shockLayerRuleset._color *= starModule.tint.ToColor();

            starGO.SetActive(true);

            return (starGO, starController, starEvolutionController, light);
        }

        public static (GameObject, Renderer, Renderer) MakeStarProxy(GameObject planet, GameObject proxyGO, StarModule starModule, IModBehaviour mod, bool isStellarRemnant)
        {
            InitPrefabs();

            var (starGO, controller, supernova) = SharedStarGeneration(proxyGO, null, mod, starModule, isStellarRemnant);

            Renderer atmosphere = null;
            Renderer fog = null;
            if (starModule.hasAtmosphere)
            {
                GameObject sunAtmosphere = Object.Instantiate(_starProxyAtmosphere, starGO.transform);
                sunAtmosphere.transform.position = proxyGO.transform.position;
                sunAtmosphere.transform.localScale = Vector3.one * OuterRadiusRatio;
                sunAtmosphere.name = "Atmosphere_Star";
                sunAtmosphere.SetActive(true);

                atmosphere = sunAtmosphere.transform.Find("Atmosphere_LOD2").GetComponent<MeshRenderer>();
                atmosphere.transform.localScale = Vector3.one;
                atmosphere.material.SetFloat(InnerRadius, starModule.size);
                atmosphere.material.SetFloat(OuterRadius, starModule.size * OuterRadiusRatio);

                fog = sunAtmosphere.transform.Find("FogSphere").GetComponent<MeshRenderer>();
                fog.transform.localScale = Vector3.one;
                fog.material.SetFloat(Radius, starModule.size * OuterRadiusRatio);

                if (starModule.tint != null)
                {
                    fog.material.SetColor(Tint, starModule.tint.ToColor());
                    atmosphere.material.SetColor(SkyColor, starModule.tint.ToColor());
                }

                controller.atmosphere = sunAtmosphere;
            }

            controller.isProxy = true;

            // Planet can have multiple stars on them, so find the one that is also a remnant / not a remnant
            // This will bug out if we make quantum stars. Will have to figure that out eventually.
            var mainController = planet.GetComponentsInChildren<StarEvolutionController>(true).Where(x => x.isRemnant == isStellarRemnant).FirstOrDefault();
            mainController.SetProxy(controller);

            // Remnants can't go supernova
            if (supernova != null)
            {
                supernova.SetIsProxy(true);                

                supernova.mainStellerDeathController = mainController.supernova;
            }

            return (starGO, atmosphere, fog);
        }

        private static (GameObject, StarEvolutionController, StellarDeathController) SharedStarGeneration(GameObject planetGO, Sector sector, IModBehaviour mod, StarModule starModule, bool isStellarRemnant)
        {
            InitPrefabs();

            var starGO = MakeStarGraphics(planetGO, sector, starModule, mod);
            starGO.SetActive(false);
            var starEvolutionController = starGO.GetComponent<StarEvolutionController>();
            starEvolutionController.isRemnant = isStellarRemnant;

            StellarDeathController supernova = null;
            var willExplode = !isStellarRemnant && starModule.stellarDeathType != StellarDeathType.None;
            if (willExplode)
            {
                if (starModule.stellarDeathType == StellarDeathType.PlanetaryNebula)
                {
                    starEvolutionController.planetaryNebula =
                    [
                         MakeSupernova(starGO, starModule, true, true),
                         MakeSupernova(starGO, starModule, true, true),
                         MakeSupernova(starGO, starModule, true, true)
                    ];
                }                                                 
                else
                {
                    supernova = MakeSupernova(starGO, starModule);
                    starEvolutionController.supernova = supernova;
                }

                starEvolutionController.supernovaSize = starModule.supernovaSize;
                var duration = starModule.supernovaSize / starModule.supernovaSpeed;
                starEvolutionController.supernovaTime = duration;
                starEvolutionController.supernovaScaleEnd = duration;
                starEvolutionController.supernovaScaleStart = duration * 0.9f;
                starEvolutionController.deathType = starModule.stellarDeathType;

                starEvolutionController.supernovaColour = starModule.supernovaTint;
                starEvolutionController.lifespan = starModule.lifespan;
            }
            starEvolutionController.willExplode = willExplode;

            starEvolutionController.enabled = true;
            starGO.SetActive(true);

            return (starGO, starEvolutionController, supernova);
        }

        public static GameObject MakeStarGraphics(GameObject rootObject, Sector sector, StarModule starModule, IModBehaviour mod)
        {
            InitPrefabs();

            var starGO = new GameObject("Star");
            starGO.transform.parent = sector?.transform ?? rootObject.transform;

            var sunSurface = Object.Instantiate(_starSurface, starGO.transform);
            sunSurface.transform.position = rootObject.transform.position;
            sunSurface.transform.localScale = Vector3.one;
            sunSurface.name = "Surface";
            sunSurface.SetActive(true);

            var solarFlareEmitter = Object.Instantiate(_starSolarFlareEmitter, starGO.transform);
            solarFlareEmitter.transform.localPosition = Vector3.zero;
            solarFlareEmitter.transform.localScale = Vector3.one;
            solarFlareEmitter.name = "SolarFlareEmitter";

            var emitter = solarFlareEmitter.GetComponent<SolarFlareEmitter>();

            if (starModule.solarFlareSettings != null)
            {
                emitter._minTimeBetweenFlares = starModule.solarFlareSettings.minTimeBetweenFlares;
                emitter._maxTimeBetweenFlares = starModule.solarFlareSettings.maxTimeBetweenFlares;
                emitter._lifeLength = starModule.solarFlareSettings.lifeLength;
            }

            if (starModule.tint != null)
            {
                emitter.tint = starModule.tint.ToColor();
            }

            var ramp = starGO.GetComponentInChildren<TessellatedSphereRenderer>(true).sharedMaterial.GetTexture(ColorRamp);

            var starEvolutionController = starGO.AddComponent<StarEvolutionController>();
            if (starModule.curve != null) starEvolutionController.SetScaleCurve(starModule.curve);
            starEvolutionController.size = starModule.size;
            starEvolutionController.startColour = starModule.tint;
            starEvolutionController.endColour = starModule.endTint;

            starEvolutionController.normalRamp = !string.IsNullOrEmpty(starModule.starRampTexture) ? ImageUtilities.GetTexture(mod, starModule.starRampTexture) : ramp;
            if (!string.IsNullOrEmpty(starModule.starCollapseRampTexture))
            {
                starEvolutionController.collapseRamp = ImageUtilities.GetTexture(mod, starModule.starCollapseRampTexture);
            }

            var material = new Material(_flareMaterial);

            // Make our own copies of all prefabs to make sure we don't actually modify them
            // else it will affect any other star using these prefabs
            // #668
            emitter._domePrefab = emitter.domePrefab.InstantiateInactive();
            emitter._loopPrefab = emitter.loopPrefab.InstantiateInactive();
            emitter._streamerPrefab = emitter.streamerPrefab.InstantiateInactive();

            // Get all possible controllers, prefabs or already created ones
            foreach (var controller in new GameObject[] { emitter.domePrefab, emitter.loopPrefab, emitter.streamerPrefab }
                .Select(x => x.GetComponent<SolarFlareController>())
                .Concat(emitter.GetComponentsInChildren<SolarFlareController>()))
            {
                // controller._meshRenderer doesn't exist yet since Awake hasn't been called
                if (starModule.tint != null)
                {
                    controller.GetComponent<MeshRenderer>().sharedMaterial = material;
                    controller._color = Color.white;
                    controller._tint = starModule.tint.ToColor();
                }
                if (starModule.solarFlareSettings != null)
                {
                    controller._scaleFactor = Vector3.one * starModule.solarFlareSettings.scaleFactor;
                }
                controller.gameObject.SetActive(true);
                controller.enabled = true;
            }

            starGO.transform.position = rootObject.transform.position;
            starGO.transform.localScale = starModule.size * Vector3.one;

            var surface = sunSurface.GetComponent<TessellatedSphereRenderer>();

            if (starModule.tint != null)
            {
                var colour = starModule.tint.ToColor();

                surface.sharedMaterial = new Material(starModule.size >= 3000 ? _giantMaterial : _mainSequenceMaterial);
                var modifier = Mathf.Max(1f, 2f * Mathf.Sqrt(starModule.solarLuminosity));
                var adjustedColour = new Color(colour.r * modifier, colour.g * modifier, colour.b * modifier);
                surface.sharedMaterial.color = adjustedColour;

                Color.RGBToHSV(adjustedColour, out var h, out var s, out var v);
                var darkenedColor = Color.HSVToRGB(h, s * 1.2f, v * 0.05f);

                if (starModule.endTint != null)
                {
                    var endColour = starModule.endTint.ToColor();
                    var adjustedEndColour = new Color(endColour.r * modifier, endColour.g * modifier, endColour.b * modifier);
                    Color.RGBToHSV(adjustedEndColour, out var hEnd, out var sEnd, out var vEnd);
                    var darkenedEndColor = Color.HSVToRGB(hEnd, sEnd * 1.2f, vEnd * 0.1f);
                    surface.sharedMaterial.SetTexture(ColorRamp, ImageUtilities.LerpGreyscaleImageAlongX(_colorOverTime, adjustedColour, darkenedColor, adjustedEndColour, darkenedEndColor));

                    if (string.IsNullOrEmpty(starModule.starCollapseRampTexture))
                    {
                        starEvolutionController.collapseRamp = ImageUtilities.LerpGreyscaleImageAlongX(_colorOverTime, adjustedEndColour, darkenedEndColor, Color.white, Color.white);
                    }
                }
                else
                {
                    surface.sharedMaterial.SetTexture(ColorRamp, ImageUtilities.LerpGreyscaleImage(_colorOverTime, adjustedColour, darkenedColor));
                }
            }

            if (!string.IsNullOrEmpty(starModule.starRampTexture))
            {
                var starRamp = ImageUtilities.GetTexture(mod, starModule.starRampTexture);
                if (starRamp != null)
                {
                    surface.sharedMaterial.SetTexture(ColorRamp, starRamp);
                }
            }

            solarFlareEmitter.SetActive(true);

            return starGO;
        }

        public static StellarDeathController MakeSupernova(GameObject starGO, StarModule starModule, bool noAudio = false, bool noSurface = false)
        {
            InitPrefabs();

            var supernovaGO = _supernovaPrefab.InstantiateInactive();
            supernovaGO.name = "Supernova";
            supernovaGO.transform.SetParent(starGO.transform);
            supernovaGO.transform.localPosition = Vector3.zero;

            var supernova = supernovaGO.GetComponent<SupernovaEffectController>();
            var stellarDeath = supernovaGO.AddComponent<StellarDeathController>();
            stellarDeath.enabled = false;
            if (!noSurface)
            {
                stellarDeath.surface = starGO.GetComponentInChildren<TessellatedSphereRenderer>(true);
            }
            var duration = starModule.supernovaSize / starModule.supernovaSpeed;
            stellarDeath.supernovaScale = new AnimationCurve(new Keyframe(0, 200, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(duration, starModule.supernovaSize, 1758.508f, 1758.508f, 1f / 3f, 1f / 3f));
            stellarDeath.supernovaAlpha = new AnimationCurve(new Keyframe(duration * 0.1f, 1, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(duration * 0.3f, 1.0002f, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(duration, 0, -0.0578f, 1 / 3f, -0.0578f, 1 / 3f));
            stellarDeath.explosionParticles = supernova._explosionParticles;
            stellarDeath.shockwave = supernova._shockwave;
            stellarDeath.shockwaveLength = supernova._shockwaveLength;
            stellarDeath.shockwaveAlpha = supernova._shockwaveAlpha;
            stellarDeath.shockwaveScale = supernova._shockwaveScale;
            stellarDeath.supernovaMaterial = supernova._supernovaMaterial;
            Object.Destroy(supernova);

            if (starModule.supernovaTint != null)
            {
                var colour = starModule.supernovaTint.ToColor();

                var supernovaMaterial = new Material(stellarDeath.supernovaMaterial);
                var ramp = ImageUtilities.LerpGreyscaleImage(_supernovaEffects, Color.white, colour);
                supernovaMaterial.SetTexture(ColorRamp, ramp);
                stellarDeath.supernovaMaterial = supernovaMaterial;

                // Motes
                var moteMaterial = supernovaGO.GetComponentInChildren<ParticleSystemRenderer>().material;
                moteMaterial.color = new Color(colour.r * 3f, colour.g * 3f, colour.b * 3f, moteMaterial.color.a);
            }

            foreach (var controller in supernovaGO.GetComponentsInChildren<SupernovaStreamersController>())
            {
                Object.DestroyImmediate(controller);
            }

            if (!noAudio)
            {
                var supernovaWallAudio = new GameObject("SupernovaWallAudio");
                supernovaWallAudio.transform.SetParent(supernovaGO.transform, false);
                supernovaWallAudio.transform.localPosition = Vector3.zero;
                supernovaWallAudio.transform.localScale = Vector3.one;
                supernovaWallAudio.layer = Layer.BasicEffectVolume;
                var audioSource = supernovaWallAudio.AddComponent<AudioSource>();
                audioSource.loop = true;
                audioSource.maxDistance = 2000;
                audioSource.dopplerLevel = 0;
                audioSource.rolloffMode = AudioRolloffMode.Custom;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1;
                audioSource.volume = 0.5f;
                audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
                stellarDeath.audioSource = supernovaWallAudio.AddComponent<OWAudioSource>();
                stellarDeath.audioSource._audioSource = audioSource;
                stellarDeath.audioSource._audioLibraryClip = AudioType.Sun_SupernovaWall_LP;
                stellarDeath.audioSource.SetTrack(OWAudioMixer.TrackName.EndTimes_SFX);
            }

            supernovaGO.SetActive(true);

            return stellarDeath;
        }
    }
}
