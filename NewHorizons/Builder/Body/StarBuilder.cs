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

namespace NewHorizons.Builder.Body
{
    static class StarBuilder
    {
        private static Texture2D _colorOverTime;
        public static StarController Make(GameObject body, Sector sector, StarModule starModule)
        {
            if (_colorOverTime == null) _colorOverTime = Main.Instance.ModHelper.Assets.GetTexture("AssetBundle/StarColorOverTime.png");

            var starGO = new GameObject("Star");
            starGO.transform.parent = body.transform;

            var sunSurface = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Geometry_SUN/Surface"), starGO.transform);
            sunSurface.transform.localPosition = Vector3.zero;
            sunSurface.transform.localScale = Vector3.one;
            sunSurface.name = "Surface";

            //var sunLight = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight"), sunGO.transform);
            var sunLight = new GameObject();
            sunLight.transform.parent = starGO.transform;
            sunLight.transform.localPosition = Vector3.zero;
            sunLight.transform.localScale = Vector3.one;
            sunLight.name = "StarLight";
            var light = sunLight.AddComponent<Light>();
            light.CopyPropertiesFrom(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>());
            light.intensity *= starModule.SolarLuminosity;

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
            sunAudio.name = "Audio_Star";

            var sunAtmosphere = GameObject.Instantiate(GameObject.Find("Sun_Body/Atmosphere_SUN"), starGO.transform);
            sunAtmosphere.transform.localPosition = Vector3.zero;
            sunAtmosphere.transform.localScale = Vector3.one;
            sunAtmosphere.name = "Atmosphere_Star";

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
            deathVolume.name = "DestructionVolume";

            PlanetaryFogController fog = sunAtmosphere.transform.Find("FogSphere").GetComponent<PlanetaryFogController>();
            TessellatedSphereRenderer surface = sunSurface.GetComponent<TessellatedSphereRenderer>();
            Light ambientLight = ambientLightGO.GetComponent<Light>();

            Color lightColour = light.color;
            if (starModule.LightTint != null) lightColour = starModule.LightTint.ToColor32();
            if (lightColour == null && starModule.Tint != null)
            {
                // Lighten it a bit
                var r = Mathf.Clamp01(starModule.Tint.R * 1.5f / 255f);
                var g = Mathf.Clamp01(starModule.Tint.G * 1.5f / 255f);
                var b = Mathf.Clamp01(starModule.Tint.B * 1.5f / 255f);
                lightColour = new Color(r, g, b);
            }
            if (lightColour != null) light.color = (Color)lightColour;

            light.color = lightColour;
            ambientLight.color = lightColour;

            fog.fogRadius = starModule.Size * 1.2f;
            if(starModule.Tint != null)
            {
                var colour = starModule.Tint.ToColor32();
                //sunLightController.sunColor = colour;
                fog.fogTint = colour;

                var sun = GameObject.Find("Sun_Body");
                var mainSequenceMaterial = sun.GetComponent<SunController>().GetValue<Material>("_startSurfaceMaterial");
                var giantMaterial = sun.GetComponent<SunController>().GetValue<Material>("_endSurfaceMaterial");

                surface.sharedMaterial = new Material(starModule.Size >= 3000 ? giantMaterial : mainSequenceMaterial);
                var mod = 8f * starModule.SolarLuminosity / 255f;
                surface.sharedMaterial.color = new Color(colour.r * mod, colour.g * mod, colour.b * mod);
                surface.sharedMaterial.SetTexture("_ColorRamp", ImageUtilities.TintImage(_colorOverTime, colour));

                sunAtmosphere.transform.Find("AtmoSphere").transform.localScale = Vector3.one * (starModule.Size + 1000)/starModule.Size;
                foreach (var lod in sunAtmosphere.transform.Find("AtmoSphere").GetComponentsInChildren<MeshRenderer>())
                {
                    lod.material.SetColor("_SkyColor", colour);
                    lod.material.SetFloat("_InnerRadius", starModule.Size);
                    lod.material.SetFloat("_OuterRadius", starModule.Size + 1000);
                }
            }

            if(starModule.SolarFlareTint != null)
                solarFlareEmitter.GetComponent<SolarFlareEmitter>().tint = starModule.SolarFlareTint.ToColor32();

            starGO.transform.localPosition = Vector3.zero;
            starGO.transform.localScale = starModule.Size * Vector3.one;

            var starController = body.AddComponent<StarController>();
            starController.Light = light;
            starController.AmbientLight = ambientLight;
            starController.FaceActiveCamera = faceActiveCamera;
            starController.CSMTextureCacher = csmTextureCacher;
            starController.ProxyShadowLight = proxyShadowLight;
            starController.Intensity = starModule.SolarLuminosity;
            starController.SunColor = lightColour;

            return starController;
        }
    }
}
