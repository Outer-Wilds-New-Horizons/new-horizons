using NewHorizons.External;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.External.VariableSize;
using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;

namespace NewHorizons.Builder.Body
{
    public static class StarBuilder
    {
        public const float OuterRadiusRatio = 1.5f;
        private static Texture2D _colorOverTime;

        public static StarController Make(GameObject planetGO, Sector sector, StarModule starModule)
        {
            var starGO = MakeStarGraphics(planetGO, sector, starModule);

            var sunAudio = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Audio_SUN"), starGO.transform);
            sunAudio.transform.localPosition = Vector3.zero;
            sunAudio.transform.localScale = Vector3.one;
            sunAudio.transform.Find("SurfaceAudio_Sun").GetComponent<AudioSource>().maxDistance = starModule.Size * 2f;
            var surfaceAudio = sunAudio.GetComponentInChildren<SunSurfaceAudioController>();
            surfaceAudio.SetSector(sector);
            surfaceAudio._sunController = null;

            sunAudio.name = "Audio_Star";

            GameObject sunAtmosphere = null;
            if (starModule.HasAtmosphere)
            {
                sunAtmosphere = GameObject.Instantiate(GameObject.Find("Sun_Body/Atmosphere_SUN"), starGO.transform);
                sunAtmosphere.transform.position = planetGO.transform.position;
                sunAtmosphere.transform.localScale = Vector3.one;
                sunAtmosphere.name = "Atmosphere_Star";
                PlanetaryFogController fog = sunAtmosphere.transform.Find("FogSphere").GetComponent<PlanetaryFogController>();
                if (starModule.Tint != null)
                {
                    fog.fogTint = starModule.Tint.ToColor();
                    sunAtmosphere.transform.Find("AtmoSphere").transform.localScale = Vector3.one * (starModule.Size * OuterRadiusRatio);
                    foreach (var lod in sunAtmosphere.transform.Find("AtmoSphere").GetComponentsInChildren<MeshRenderer>())
                    {
                        lod.material.SetColor("_SkyColor", starModule.Tint.ToColor());
                        lod.material.SetFloat("_InnerRadius", starModule.Size);
                        lod.material.SetFloat("_OuterRadius", starModule.Size * OuterRadiusRatio);
                    }
                }
                fog.transform.localScale = Vector3.one;
                fog.fogRadius = starModule.Size * OuterRadiusRatio;
                fog.lodFadeDistance = fog.fogRadius * (StarBuilder.OuterRadiusRatio - 1f);
            }

            var ambientLightGO = GameObject.Instantiate(GameObject.Find("Sun_Body/AmbientLight_SUN"), starGO.transform);
            ambientLightGO.transform.localPosition = Vector3.zero;
            ambientLightGO.name = "AmbientLight_Star";

            var heatVolume = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Volumes_SUN/HeatVolume"), starGO.transform);
            heatVolume.transform.localPosition = Vector3.zero;
            heatVolume.transform.localScale = Vector3.one;
            heatVolume.GetComponent<SphereShape>().radius = 1f;
            heatVolume.name = "HeatVolume";

            var deathVolume = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Volumes_SUN/ScaledVolumesRoot/DestructionFluidVolume"), starGO.transform);
            deathVolume.transform.localPosition = Vector3.zero;
            deathVolume.transform.localScale = Vector3.one;
            deathVolume.GetComponent<SphereCollider>().radius = 1f;
            deathVolume.GetComponent<DestructionVolume>()._onlyAffectsPlayerAndShip = false;
            deathVolume.GetComponent<DestructionVolume>()._shrinkBodies = false;
            deathVolume.name = "DestructionVolume";

            Light ambientLight = ambientLightGO.GetComponent<Light>();

            var sunLight = new GameObject();
            sunLight.transform.parent = starGO.transform;
            sunLight.transform.localPosition = Vector3.zero;
            sunLight.transform.localScale = Vector3.one;
            sunLight.name = "StarLight";

            var light = sunLight.AddComponent<Light>();
            light.CopyPropertiesFrom(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>());
            light.intensity *= starModule.SolarLuminosity;
            light.range *= Mathf.Sqrt(starModule.SolarLuminosity);

            Color lightColour = light.color;
            if (starModule.LightTint != null) lightColour = starModule.LightTint.ToColor();
            if (lightColour == null && starModule.Tint != null)
            {
                // Lighten it a bit
                var r = Mathf.Clamp01(starModule.Tint.R * 1.5f);
                var g = Mathf.Clamp01(starModule.Tint.G * 1.5f);
                var b = Mathf.Clamp01(starModule.Tint.B * 1.5f);
                lightColour = new Color(r, g, b);
            }
            if (lightColour != null) light.color = (Color)lightColour;

            light.color = lightColour;
            ambientLight.color = lightColour;

            var faceActiveCamera = sunLight.AddComponent<FaceActiveCamera>();
            faceActiveCamera.CopyPropertiesFrom(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<FaceActiveCamera>());
            var csmTextureCacher = sunLight.AddComponent<CSMTextureCacher>();
            csmTextureCacher.CopyPropertiesFrom(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<CSMTextureCacher>());
            csmTextureCacher._light = light;
            var proxyShadowLight = sunLight.AddComponent<ProxyShadowLight>();
            proxyShadowLight.CopyPropertiesFrom(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<ProxyShadowLight>());
            proxyShadowLight._light = light;

            StarController starController = null;
            if (starModule.SolarLuminosity != 0)
            {
                starController = planetGO.AddComponent<StarController>();
                starController.Light = light;
                starController.AmbientLight = ambientLight;
                starController.FaceActiveCamera = faceActiveCamera;
                starController.CSMTextureCacher = csmTextureCacher;
                starController.ProxyShadowLight = proxyShadowLight;
                starController.Intensity = starModule.SolarLuminosity;
                starController.SunColor = lightColour;
            }

            var supernova = MakeSupernova(starGO);

            var controller = starGO.AddComponent<StarEvolutionController>();
            if(starModule.Curve != null) controller.scaleCurve = starModule.ToAnimationCurve();
            controller.size = starModule.Size;
            controller.atmosphere = sunAtmosphere;
            controller.supernova = supernova;
            controller.startColour = starModule.Tint;
            controller.startColour = starModule.EndTint;

            return starController;
        }

        public static GameObject MakeStarProxy(GameObject planet, GameObject proxyGO, StarModule starModule)
        {
            var starGO = new GameObject("Star");
            starGO.transform.parent = proxyGO.transform;
            starGO.transform.localPosition = Vector3.zero;

            MakeStarGraphics(proxyGO, null, starModule);

            var supernova = MakeSupernova(starGO);

            var controller = starGO.AddComponent<StarEvolutionController>();
            if (starModule.Curve != null) controller.scaleCurve = starModule.ToAnimationCurve();
            controller.size = starModule.Size;
            controller.supernova = supernova;

            planet.GetComponentInChildren<StarEvolutionController>().SetProxy(controller);

            return proxyGO;
        }

        public static GameObject MakeStarGraphics(GameObject rootObject, Sector sector, StarModule starModule)
        {
            if (_colorOverTime == null) _colorOverTime = ImageUtilities.GetTexture(Main.Instance, "AssetBundle/StarColorOverTime.png");

            var starGO = new GameObject("Star");
            starGO.transform.parent = sector?.transform ?? rootObject.transform;

            var sunSurface = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Geometry_SUN/Surface"), starGO.transform);
            sunSurface.transform.position = rootObject.transform.position;
            sunSurface.transform.localScale = Vector3.one;
            sunSurface.name = "Surface";

            var solarFlareEmitter = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SolarFlareEmitter"), starGO.transform);
            solarFlareEmitter.transform.localPosition = Vector3.zero;
            solarFlareEmitter.transform.localScale = Vector3.one;
            solarFlareEmitter.name = "SolarFlareEmitter";

            if (starModule.SolarFlareTint != null)
            {
                solarFlareEmitter.GetComponent<SolarFlareEmitter>().tint = starModule.SolarFlareTint.ToColor();
            }

            starGO.transform.position = rootObject.transform.position;
            starGO.transform.localScale = starModule.Size * Vector3.one;

            if (starModule.Tint != null)
            {
                TessellatedSphereRenderer surface = sunSurface.GetComponent<TessellatedSphereRenderer>();

                var colour = starModule.Tint.ToColor();

                var sun = GameObject.Find("Sun_Body");
                var mainSequenceMaterial = sun.GetComponent<SunController>().GetValue<Material>("_startSurfaceMaterial");
                var giantMaterial = sun.GetComponent<SunController>().GetValue<Material>("_endSurfaceMaterial");

                surface.sharedMaterial = new Material(starModule.Size >= 3000 ? giantMaterial : mainSequenceMaterial);
                var mod = Mathf.Max(0.5f, 2f * Mathf.Sqrt(starModule.SolarLuminosity));
                var adjustedColour = new Color(colour.r * mod, colour.g * mod, colour.b * mod);
                surface.sharedMaterial.color = adjustedColour;

                Color.RGBToHSV(adjustedColour, out float H, out float S, out float V);
                var darkenedColor = Color.HSVToRGB(H, S, V * 0.05f);
                surface.sharedMaterial.SetTexture("_ColorRamp", ImageUtilities.LerpGreyscaleImage(_colorOverTime, adjustedColour, darkenedColor));
            }

            return starGO;
        }

        public static SupernovaEffectController MakeSupernova(GameObject starGO)
        {
            var supernovaGO = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/Supernova").InstantiateInactive();
            supernovaGO.transform.SetParent(starGO.transform);
            supernovaGO.transform.localPosition = Vector3.zero;

            var supernova = supernovaGO.GetComponent<SupernovaEffectController>();
            supernova._surface = starGO.GetComponentInChildren<TessellatedSphereRenderer>();
            supernova._supernovaVolume = null;

            supernovaGO.SetActive(true);

            return supernova;
        }
    }
}
