using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;
using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using NewHorizons.External.Modules.VariableSize;
using Logger = NewHorizons.Utility.Logger;
using OWML.ModHelper;
using OWML.Common;

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

        public static StarController Make(GameObject planetGO, Sector sector, StarModule starModule, IModBehaviour mod)
        {
            var starGO = MakeStarGraphics(planetGO, sector, starModule, mod);
            var ramp = starGO.GetComponentInChildren<TessellatedSphereRenderer>().sharedMaterial.GetTexture(ColorRamp);

            var sunAudio = Object.Instantiate(SearchUtilities.Find("Sun_Body/Sector_SUN/Audio_SUN"), starGO.transform);
            sunAudio.transform.localPosition = Vector3.zero;
            sunAudio.transform.localScale = Vector3.one;
            sunAudio.transform.Find("SurfaceAudio_Sun").GetComponent<AudioSource>().maxDistance = starModule.size * 2f;
            var sunSurfaceAudio = sunAudio.GetComponentInChildren<SunSurfaceAudioController>();
            var surfaceAudio = sunSurfaceAudio.gameObject.AddComponent<StarSurfaceAudioController>();
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
                var fog = sunAtmosphere.transform.Find("FogSphere").GetComponent<PlanetaryFogController>();
                if (starModule.tint != null)
                {
                    fog.fogTint = starModule.tint.ToColor();
                    fog.fogImpostor.material.SetColor(Tint, starModule.tint.ToColor());
                    sunAtmosphere.transform.Find("AtmoSphere").transform.localScale = Vector3.one;
                    foreach (var lod in sunAtmosphere.transform.Find("AtmoSphere").GetComponentsInChildren<MeshRenderer>())
                    {
                        lod.material.SetColor(SkyColor, starModule.tint.ToColor());
                        lod.material.SetFloat(InnerRadius, starModule.size);
                        lod.material.SetFloat(OuterRadius, starModule.size * OuterRadiusRatio);
                    }
                }
                fog.transform.localScale = Vector3.one;
                fog.fogRadius = starModule.size * OuterRadiusRatio;
                fog.lodFadeDistance = fog.fogRadius * (StarBuilder.OuterRadiusRatio - 1f);
                fog.fogImpostor.material.SetFloat(Radius, starModule.size * OuterRadiusRatio);
            }

            var ambientLightGO = Object.Instantiate(SearchUtilities.Find("Sun_Body/AmbientLight_SUN"), starGO.transform);
            ambientLightGO.transform.localPosition = Vector3.zero;
            ambientLightGO.name = "AmbientLight_Star";

            Light ambientLight = ambientLightGO.GetComponent<Light>();
            ambientLight.range = starModule.size * OuterRadiusRatio;

            var heatVolume = Object.Instantiate(SearchUtilities.Find("Sun_Body/Sector_SUN/Volumes_SUN/HeatVolume"), starGO.transform);
            heatVolume.transform.localPosition = Vector3.zero;
            heatVolume.transform.localScale = Vector3.one;
            heatVolume.GetComponent<SphereShape>().radius = 1f;
            heatVolume.name = "HeatVolume";

            var deathVolume = Object.Instantiate(SearchUtilities.Find("Sun_Body/Sector_SUN/Volumes_SUN/ScaledVolumesRoot/DestructionFluidVolume"), starGO.transform);
            deathVolume.transform.localPosition = Vector3.zero;
            deathVolume.transform.localScale = Vector3.one;
            deathVolume.GetComponent<SphereCollider>().radius = 1f;
            deathVolume.GetComponent<DestructionVolume>()._onlyAffectsPlayerAndShip = true;
            deathVolume.GetComponent<DestructionVolume>()._shrinkBodies = true;
            deathVolume.name = "DestructionVolume";

            var planetDestructionVolume = Object.Instantiate(deathVolume, starGO.transform);
            planetDestructionVolume.transform.localPosition = Vector3.zero;
            planetDestructionVolume.transform.localScale = Vector3.one;
            planetDestructionVolume.GetComponent<SphereCollider>().radius = 0.8f;
            planetDestructionVolume.GetComponent<DestructionVolume>()._onlyAffectsPlayerAndShip = false;
            planetDestructionVolume.GetComponent<DestructionVolume>()._shrinkBodies = true;
            planetDestructionVolume.name = "PlanetDestructionVolume";

            var sunLight = new GameObject("StarLight");
            sunLight.transform.parent = starGO.transform;
            sunLight.transform.localPosition = Vector3.zero;
            sunLight.transform.localScale = Vector3.one;

            var light = sunLight.AddComponent<Light>();
            light.CopyPropertiesFrom(SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>());
            light.intensity *= starModule.solarLuminosity;
            light.range = starModule.lightRadius;
            light.range *= Mathf.Sqrt(starModule.solarLuminosity);

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
            }

            var stellarRemnant = MakeStellarRemnant(starGO, starModule);

            var supernova = MakeSupernova(starGO, starModule);

            starGO.SetActive(false);
            var controller = starGO.AddComponent<StarEvolutionController>();
            if (starModule.curve != null) controller.SetScaleCurve(starModule.curve);
            controller.size = starModule.size;
            controller.atmosphere = sunAtmosphere;
            controller.controller = starController;
            controller.supernova = supernova;
            controller.StartColour = starModule.tint;
            controller.EndColour = starModule.endTint;
            controller.SupernovaColour = starModule.supernovaTint;
            controller.WillExplode = starModule.goSupernova;
            controller.lifespan = starModule.lifespan;
            controller.normalRamp = !string.IsNullOrEmpty(starModule.starRampTexture) ? ImageUtilities.GetTexture(mod, starModule.starRampTexture) : ramp;
            controller._destructionVolume = deathVolume.GetComponent<DestructionVolume>();
            controller._planetDestructionVolume = planetDestructionVolume.GetComponent<DestructionVolume>();
            controller._destructionFluidVolume = planetDestructionVolume.GetComponent<SimpleFluidVolume>();
            controller._planetDestructionFluidVolume = planetDestructionVolume.GetComponent<SimpleFluidVolume>();
            if (!string.IsNullOrEmpty(starModule.starCollapseRampTexture))
            {
                controller.collapseRamp = ImageUtilities.GetTexture(mod, starModule.starCollapseRampTexture);
            }
            surfaceAudio.SetStarEvolutionController(controller);
            starGO.SetActive(true);

            // It fucking insists on this existing and its really annoying
            var supernovaVolume = new GameObject("SupernovaVolumePlaceholder");
            supernovaVolume.transform.SetParent(starGO.transform);
            supernovaVolume.layer = LayerMask.NameToLayer("BasicEffectVolume");
            var sphere = supernovaVolume.AddComponent<SphereCollider>();
            sphere.radius = 0f;
            sphere.isTrigger = true;
            supernovaVolume.AddComponent<OWCollider>();
            supernova._supernovaVolume = supernovaVolume.AddComponent<SupernovaDestructionVolume>();

            var shockLayerRuleset = sector.gameObject.AddComponent<ShockLayerRuleset>();
            shockLayerRuleset._type = ShockLayerRuleset.ShockType.Radial;
            shockLayerRuleset._trailLength = 50;
            shockLayerRuleset._radialCenter = deathVolume.transform;
            shockLayerRuleset._innerRadius = 0;
            shockLayerRuleset._outerRadius = starModule.size * OuterRadiusRatio;
            if (starModule.tint != null) shockLayerRuleset._color *= starModule.tint.ToColor();

            return starController;
        }

        public static GameObject MakeStarProxy(GameObject planet, GameObject proxyGO, StarModule starModule, IModBehaviour mod)
        {
            var starGO = MakeStarGraphics(proxyGO, null, starModule, mod);
            var ramp = starGO.GetComponentInChildren<TessellatedSphereRenderer>().sharedMaterial.GetTexture(ColorRamp);

            var supernova = MakeSupernova(starGO, starModule);

            supernova._belongsToProxySun = true;

            starGO.SetActive(false);
            var controller = starGO.AddComponent<StarEvolutionController>();
            if (starModule.curve != null) controller.SetScaleCurve(starModule.curve);
            controller.size = starModule.size;
            controller.supernovaSize = starModule.supernovaSize;
            controller.supernova = supernova;
            controller.StartColour = starModule.tint;
            controller.EndColour = starModule.endTint;
            controller.SupernovaColour = starModule.supernovaTint;
            controller.WillExplode = starModule.goSupernova;
            controller.lifespan = starModule.lifespan;
            controller.normalRamp = !string.IsNullOrEmpty(starModule.starRampTexture) ? ImageUtilities.GetTexture(mod, starModule.starRampTexture) : ramp;
            if (!string.IsNullOrEmpty(starModule.starCollapseRampTexture))
            {
                controller.collapseRamp = ImageUtilities.GetTexture(mod, starModule.starCollapseRampTexture);
            }
            controller.enabled = true;
            starGO.SetActive(true);

            planet.GetComponentInChildren<StarEvolutionController>(true).SetProxy(controller);

            return proxyGO;
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

        public static SupernovaEffectController MakeSupernova(GameObject starGO, StarModule starModule)
        {
            var supernovaGO = SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/Supernova").InstantiateInactive();
            supernovaGO.name = "Supernova";
            supernovaGO.transform.SetParent(starGO.transform);
            supernovaGO.transform.localPosition = Vector3.zero;

            var supernova = supernovaGO.GetComponent<SupernovaEffectController>();
            supernova._surface = starGO.GetComponentInChildren<TessellatedSphereRenderer>();
            supernova._supernovaScale = new AnimationCurve(new Keyframe(0, 200, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(45, starModule.supernovaSize, 1758.508f, 1758.508f, 1f / 3f, 1f / 3f));
            supernova._supernovaVolume = null;

            if (starModule.supernovaTint != null)
            {
                var colour = starModule.supernovaTint.ToColor();

                var supernovaMaterial = new Material(supernova._supernovaMaterial);
                var ramp = ImageUtilities.LerpGreyscaleImage(ImageUtilities.GetTexture(Main.Instance, "Assets/textures/Effects_SUN_Supernova_d.png"), Color.white, colour);
                supernovaMaterial.SetTexture(ColorRamp, ramp);
                supernova._supernovaMaterial = supernovaMaterial;

                // Motes
                var moteMaterial = supernova.GetComponentInChildren<ParticleSystemRenderer>().material;
                moteMaterial.color = new Color(colour.r * 3f, colour.g * 3f, colour.b * 3f, moteMaterial.color.a);
            }

            foreach (var controller in supernova.GetComponentsInChildren<SupernovaStreamersController>())
            {
                Object.DestroyImmediate(controller);
            }

            supernovaGO.SetActive(true);

            return supernova;
        }

        public static GameObject MakeStellarRemnant(GameObject starGO, StarModule starModule)
        {
            GameObject stellarRemnant = new GameObject("StellarRemnant");
            stellarRemnant.transform.SetParent(starGO.transform, false);
            return stellarRemnant;
        }
    }
}
