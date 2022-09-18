using NewHorizons.Components.SizeControllers;
using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using NewHorizons.External.Modules.VariableSize;
using Logger = NewHorizons.Utility.Logger;
using OWML.ModHelper;
using OWML.Common;
using UnityEngine.InputSystem.XR;
using System.Linq;
using NewHorizons.Components.Stars;

namespace NewHorizons.Builder.Body
{
    public static class StarBuilder
    {
        public const float OuterRadiusRatio = 1.5f;
        private static Texture2D _colorOverTime;
        private static readonly int ColorRamp = Shader.PropertyToID("_ColorRamp");
        private static readonly int SkyColor = Shader.PropertyToID("_SkyColor");
        private static readonly int Tint = Shader.PropertyToID("_Tint");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");
        private static readonly int OuterRadius = Shader.PropertyToID("_OuterRadius");

        public static (GameObject, StarController, StarEvolutionController) Make(GameObject planetGO, Sector sector, StarModule starModule, IModBehaviour mod, bool isStellarRemnant)
        {
            var (starGO, starEvolutionController, supernova) = SharedStarGeneration(planetGO, sector, mod, starModule, isStellarRemnant);

            var sunAudio = Object.Instantiate(SearchUtilities.Find("Sun_Body/Sector_SUN/Audio_SUN"), starGO.transform);
            sunAudio.transform.localPosition = Vector3.zero;
            sunAudio.transform.localScale = Vector3.one;
            sunAudio.transform.Find("SurfaceAudio_Sun").GetComponent<AudioSource>().maxDistance = starModule.size * 2f;
            var sunSurfaceAudio = sunAudio.GetComponentInChildren<SunSurfaceAudioController>();
            var surfaceAudio = sunSurfaceAudio.gameObject.AddComponent<StarSurfaceAudioController>();
            surfaceAudio._size = starModule.size;
            GameObject.Destroy(sunSurfaceAudio);
            surfaceAudio.SetSector(sector);

            sunAudio.name = "Audio_Star";

            GameObject sunAtmosphere = null;
            if (starModule.hasAtmosphere)
            {
                sunAtmosphere = Object.Instantiate(SearchUtilities.Find("Sun_Body/Atmosphere_SUN"), starGO.transform);
                sunAtmosphere.transform.position = planetGO.transform.position;
                sunAtmosphere.transform.localScale = Vector3.one * OuterRadiusRatio;
                sunAtmosphere.name = "Atmosphere_Star";

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
                fog.lodFadeDistance = fog.fogRadius * (StarBuilder.OuterRadiusRatio - 1f);

                fog.fogImpostor.material.SetFloat(Radius, starModule.size * OuterRadiusRatio);
                if (starModule.tint != null)
                {
                    fog.fogTint = starModule.tint.ToColor();
                    fog.fogImpostor.material.SetColor(Tint, starModule.tint.ToColor());
                    foreach (var lod in lods)
                        lod.material.SetColor(SkyColor, starModule.tint.ToColor());
                }
            }

            var ambientLightGO = Object.Instantiate(SearchUtilities.Find("Sun_Body/AmbientLight_SUN"), starGO.transform);
            ambientLightGO.transform.localPosition = Vector3.zero;
            ambientLightGO.name = "AmbientLight_Star";

            Light ambientLight = ambientLightGO.GetComponent<Light>();
            ambientLight.range = starModule.size * OuterRadiusRatio;

            var heatVolume = new GameObject("HeatVolume");
            heatVolume.transform.SetParent(starGO.transform, false);
            heatVolume.transform.localPosition = Vector3.zero;
            heatVolume.transform.localScale = Vector3.one;
            heatVolume.layer = LayerMask.NameToLayer("BasicEffectVolume");
            heatVolume.AddComponent<SphereShape>().radius = 1.1f;
            heatVolume.AddComponent<OWTriggerVolume>();
            heatVolume.AddComponent<HeatHazardVolume>()._damagePerSecond = 20f;

            // Kill player entering the star
            var deathVolume = new GameObject("DestructionFluidVolume");
            deathVolume.transform.SetParent(starGO.transform, false);
            deathVolume.transform.localPosition = Vector3.zero;
            deathVolume.transform.localScale = Vector3.one;
            deathVolume.layer = LayerMask.NameToLayer("BasicEffectVolume");
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
            planetDestructionVolume.layer = LayerMask.NameToLayer("BasicEffectVolume");
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
            light.CopyPropertiesFrom(SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>());
            light.intensity *= starModule.solarLuminosity;
            light.range = starModule.lightRadius;

            Color lightColour = light.color;
            if (starModule.lightTint != null) lightColour = starModule.lightTint.ToColor();

            light.color = lightColour;
            ambientLight.color = new Color(lightColour.r, lightColour.g, lightColour.b, lightColour.a == 0 ? 0.0001f : lightColour.a);

            var faceActiveCamera = sunLight.AddComponent<FaceActiveCamera>();
            faceActiveCamera.CopyPropertiesFrom(SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<FaceActiveCamera>());
            var csmTextureCacher = sunLight.AddComponent<CSMTextureCacher>();
            csmTextureCacher.CopyPropertiesFrom(SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<CSMTextureCacher>());
            csmTextureCacher._light = light;
            var proxyShadowLight = sunLight.AddComponent<ProxyShadowLight>();
            proxyShadowLight.CopyPropertiesFrom(SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<ProxyShadowLight>());
            proxyShadowLight._light = light;

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

            return (starGO, starController, starEvolutionController);
        }

        public static (GameObject, Renderer, Renderer) MakeStarProxy(GameObject planet, GameObject proxyGO, StarModule starModule, IModBehaviour mod, bool isStellarRemnant)
        {
            var (starGO, controller, supernova) = SharedStarGeneration(proxyGO, null, mod, starModule, isStellarRemnant);

            Renderer atmosphere = null;
            Renderer fog = null;
            if (starModule.hasAtmosphere)
            {
                GameObject sunAtmosphere = Object.Instantiate(SearchUtilities.Find("SunProxy/Sun_Proxy_Body/Atmosphere_SUN", false) ?? SearchUtilities.Find("SunProxy(Clone)/Sun_Proxy_Body/Atmosphere_SUN"), starGO.transform);
                sunAtmosphere.transform.position = proxyGO.transform.position;
                sunAtmosphere.transform.localScale = Vector3.one * OuterRadiusRatio;
                sunAtmosphere.name = "Atmosphere_Star";

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
            var starGO = MakeStarGraphics(planetGO, sector, starModule, mod);
            starGO.SetActive(false);

            var ramp = starGO.GetComponentInChildren<TessellatedSphereRenderer>(true).sharedMaterial.GetTexture(ColorRamp);

            var starEvolutionController = starGO.AddComponent<StarEvolutionController>();
            starEvolutionController.isRemnant = isStellarRemnant;
            if (starModule.curve != null) starEvolutionController.SetScaleCurve(starModule.curve);
            starEvolutionController.size = starModule.size;
            starEvolutionController.startColour = starModule.tint;
            starEvolutionController.endColour = starModule.endTint;

            starEvolutionController.normalRamp = !string.IsNullOrEmpty(starModule.starRampTexture) ? ImageUtilities.GetTexture(mod, starModule.starRampTexture) : ramp;
            if (!string.IsNullOrEmpty(starModule.starCollapseRampTexture))
            {
                starEvolutionController.collapseRamp = ImageUtilities.GetTexture(mod, starModule.starCollapseRampTexture);
            }

            StellarDeathController supernova = null;
            var willExplode = !isStellarRemnant && starModule.stellarDeathType != StellarDeathType.None;
            if (willExplode)
            {
                supernova = MakeSupernova(starGO, starModule);

                starEvolutionController.supernovaSize = starModule.supernovaSize;
                var duration = starModule.supernovaSize / starModule.supernovaSpeed;
                starEvolutionController.supernovaTime = duration;
                starEvolutionController.supernovaScaleEnd = duration;
                starEvolutionController.supernovaScaleStart = duration * 0.9f;
                starEvolutionController.deathType = starModule.stellarDeathType;
                starEvolutionController.supernova = supernova;
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
            if (_colorOverTime == null) _colorOverTime = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/StarColorOverTime.png");

            var starGO = new GameObject("Star");
            starGO.transform.parent = sector?.transform ?? rootObject.transform;

            var sunSurface = Object.Instantiate(SearchUtilities.Find("Sun_Body/Sector_SUN/Geometry_SUN/Surface"), starGO.transform);
            sunSurface.transform.position = rootObject.transform.position;
            sunSurface.transform.localScale = Vector3.one;
            sunSurface.name = "Surface";

            var solarFlareEmitter = Object.Instantiate(SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SolarFlareEmitter"), starGO.transform);
            solarFlareEmitter.transform.localPosition = Vector3.zero;
            solarFlareEmitter.transform.localScale = Vector3.one;
            solarFlareEmitter.name = "SolarFlareEmitter";

            if (starModule.tint != null)
            {
                var flareTint = starModule.tint.ToColor();
                var emitter = solarFlareEmitter.GetComponent<SolarFlareEmitter>();
                emitter.tint = flareTint;
                foreach (var controller in solarFlareEmitter.GetComponentsInChildren<SolarFlareController>())
                {
                    // It multiplies color by tint but wants something very bright idk
                    controller._color = new Color(1, 1, 1);
                    controller.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Color", controller._color);
                    controller._tint = flareTint;
                }
            }

            starGO.transform.position = rootObject.transform.position;
            starGO.transform.localScale = starModule.size * Vector3.one;

            TessellatedSphereRenderer surface = sunSurface.GetComponent<TessellatedSphereRenderer>();

            if (starModule.tint != null)
            {
                var colour = starModule.tint.ToColor();

                var sun = SearchUtilities.Find("Sun_Body");
                var mainSequenceMaterial = sun.GetComponent<SunController>()._startSurfaceMaterial;
                var giantMaterial = sun.GetComponent<SunController>()._endSurfaceMaterial;

                surface.sharedMaterial = new Material(starModule.size >= 3000 ? giantMaterial : mainSequenceMaterial);
                var modifier = Mathf.Max(1f, 2f * Mathf.Sqrt(starModule.solarLuminosity));
                var adjustedColour = new Color(colour.r * modifier, colour.g * modifier, colour.b * modifier);
                surface.sharedMaterial.color = adjustedColour;

                Color.RGBToHSV(adjustedColour, out var h, out var s, out var v);
                var darkenedColor = Color.HSVToRGB(h, s * 1.2f, v * 0.05f);

                if (starModule.endTint != null)
                {
                    var endColour = starModule.endTint.ToColor();
                    darkenedColor = new Color(endColour.r * modifier, endColour.g * modifier, endColour.b * modifier);
                }

                surface.sharedMaterial.SetTexture(ColorRamp, ImageUtilities.LerpGreyscaleImage(_colorOverTime, adjustedColour, darkenedColor));
            }

            if (!string.IsNullOrEmpty(starModule.starRampTexture))
            {
                var ramp = ImageUtilities.GetTexture(mod, starModule.starRampTexture);
                if (ramp != null)
                {
                    surface.sharedMaterial.SetTexture(ColorRamp, ramp);
                }
            }

            return starGO;
        }

        public static StellarDeathController MakeSupernova(GameObject starGO, StarModule starModule, bool noAudio = false)
        {
            var supernovaGO = SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/Supernova").InstantiateInactive();
            supernovaGO.name = "Supernova";
            supernovaGO.transform.SetParent(starGO.transform);
            supernovaGO.transform.localPosition = Vector3.zero;

            var supernova = supernovaGO.GetComponent<SupernovaEffectController>();
            var stellarDeath = supernovaGO.AddComponent<StellarDeathController>();
            stellarDeath.enabled = false; 
            stellarDeath.surface = starGO.GetComponentInChildren<TessellatedSphereRenderer>(true);
            var duration = starModule.supernovaSize / starModule.supernovaSpeed;
            stellarDeath.supernovaScale = new AnimationCurve(new Keyframe(0, 200, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(duration, starModule.supernovaSize, 1758.508f, 1758.508f, 1f / 3f, 1f / 3f));
            stellarDeath.supernovaAlpha = new AnimationCurve(new Keyframe(duration * 0.1f, 1, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(duration * 0.3f, 1.0002f, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(duration, 0, -0.0578f, 1 / 3f, -0.0578f, 1 / 3f));
            stellarDeath.explosionParticles = supernova._explosionParticles;
            stellarDeath.shockwave = supernova._shockwave;
            stellarDeath.shockwaveLength = supernova._shockwaveLength;
            stellarDeath.shockwaveAlpha = supernova._shockwaveAlpha;
            stellarDeath.shockwaveScale = supernova._shockwaveScale;
            stellarDeath.supernovaMaterial = supernova._supernovaMaterial;
            GameObject.Destroy(supernova);

            if (starModule.supernovaTint != null)
            {
                var colour = starModule.supernovaTint.ToColor();

                var supernovaMaterial = new Material(stellarDeath.supernovaMaterial);
                var ramp = ImageUtilities.LerpGreyscaleImage(ImageUtilities.GetTexture(Main.Instance, "Assets/textures/Effects_SUN_Supernova_d.png"), Color.white, colour);
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
                supernovaWallAudio.layer = LayerMask.NameToLayer("BasicEffectVolume");
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
