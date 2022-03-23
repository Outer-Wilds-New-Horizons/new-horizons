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
    static class StarBuilder
    {
        public const float OuterRadiusRatio = 1.5f;

        private static Texture2D _colorOverTime;
        public static StarController Make(GameObject body, Sector sector, StarModule starModule)
        {
            if (_colorOverTime == null) _colorOverTime = ImageUtilities.GetTexture(Main.Instance, "AssetBundle/StarColorOverTime.png");

            var starGO = new GameObject("Star");
            starGO.transform.parent = body.transform;

            var sunSurface = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Geometry_SUN/Surface"), starGO.transform);
            sunSurface.transform.localPosition = Vector3.zero;
            sunSurface.transform.localScale = Vector3.one;
            sunSurface.name = "Surface";

            var sunLight = new GameObject();
            sunLight.transform.parent = starGO.transform;
            sunLight.transform.localPosition = Vector3.zero;
            sunLight.transform.localScale = Vector3.one;
            sunLight.name = "StarLight";
            var light = sunLight.AddComponent<Light>();
            light.CopyPropertiesFrom(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>());
            light.intensity *= starModule.SolarLuminosity;
            light.range *= Mathf.Sqrt(starModule.SolarLuminosity);

            var faceActiveCamera = sunLight.AddComponent<FaceActiveCamera>();
            faceActiveCamera.CopyPropertiesFrom(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<FaceActiveCamera>());
            var csmTextureCacher = sunLight.AddComponent<CSMTextureCacher>();
            csmTextureCacher.CopyPropertiesFrom(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<CSMTextureCacher>());
            csmTextureCacher._light = light;
            var proxyShadowLight = sunLight.AddComponent<ProxyShadowLight>();
            proxyShadowLight.CopyPropertiesFrom(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<ProxyShadowLight>());
            proxyShadowLight._light = light;

            var solarFlareEmitter = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SolarFlareEmitter"), starGO.transform);
            solarFlareEmitter.transform.localPosition = Vector3.zero;
            solarFlareEmitter.transform.localScale = Vector3.one;
            solarFlareEmitter.name = "SolarFlareEmitter";

            var sunAudio = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Audio_SUN"), starGO.transform);
            sunAudio.transform.localPosition = Vector3.zero;
            sunAudio.transform.localScale = Vector3.one;
            sunAudio.transform.Find("SurfaceAudio_Sun").GetComponent<AudioSource>().maxDistance = starModule.Size * 2f;
            var surfaceAudio = sunAudio.GetComponentInChildren<SunSurfaceAudioController>();
            surfaceAudio.SetSector(sector);
            surfaceAudio._sunController = null;

            sunAudio.name = "Audio_Star";

            if(starModule.HasAtmosphere)
            {
                var sunAtmosphere = GameObject.Instantiate(GameObject.Find("Sun_Body/Atmosphere_SUN"), body.transform);
                sunAtmosphere.transform.localPosition = Vector3.zero;
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
                if (starModule.Curve != null)
                {
                    var controller = sunAtmosphere.AddComponent<StarAtmosphereSizeController>();
                    controller.scaleCurve = starModule.ToAnimationCurve();
                    controller.initialSize = starModule.Size;
                }
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

            TessellatedSphereRenderer surface = sunSurface.GetComponent<TessellatedSphereRenderer>();
            Light ambientLight = ambientLightGO.GetComponent<Light>();

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

            if(starModule.Tint != null)
            {
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

            if(starModule.SolarFlareTint != null)
                solarFlareEmitter.GetComponent<SolarFlareEmitter>().tint = starModule.SolarFlareTint.ToColor();

            starGO.transform.localPosition = Vector3.zero;
            starGO.transform.localScale = starModule.Size * Vector3.one;

            StarController starController = null;
            if (starModule.SolarLuminosity != 0)
            {
                starController = body.AddComponent<StarController>();
                starController.Light = light;
                starController.AmbientLight = ambientLight;
                starController.FaceActiveCamera = faceActiveCamera;
                starController.CSMTextureCacher = csmTextureCacher;
                starController.ProxyShadowLight = proxyShadowLight;
                starController.Intensity = starModule.SolarLuminosity;
                starController.SunColor = lightColour;
            }

            if (starModule.Curve != null)
            {
                var levelController = starGO.AddComponent<SandLevelController>();
                var curve = new AnimationCurve();
                foreach (var pair in starModule.Curve)
                {
                    curve.AddKey(new Keyframe(pair.Time, starModule.Size * pair.Value));
                }
                levelController._scaleCurve = curve;
            }

            return starController;
        }
    }
}
