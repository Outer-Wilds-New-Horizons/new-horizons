using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Body
{
    static class StarBuilder
    {
        public static void Make(GameObject body, Sector sector, StarModule starModule)
        {
            var sunGO = new GameObject("Star");
            sunGO.transform.parent = body.transform;

            var sunSurface = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Geometry_SUN/Surface"), sunGO.transform);
            sunSurface.transform.localPosition = Vector3.zero;
            sunSurface.transform.localScale = Vector3.one;
            sunSurface.name = "Surface";

            var sunLight = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight"), sunGO.transform);
            sunSurface.transform.localPosition = Vector3.zero;
            sunSurface.transform.localScale = Vector3.one;
            sunLight.name = "StarLight";

            var solarFlareEmitter = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SolarFlareEmitter"), sunGO.transform);
            sunSurface.transform.localPosition = Vector3.zero;
            sunSurface.transform.localScale = Vector3.one;
            solarFlareEmitter.name = "SolarFlareEmitter";

            var sunAudio = GameObject.Instantiate(GameObject.Find("Sun_Body/Sector_SUN/Audio_SUN"), sunGO.transform);
            sunSurface.transform.localPosition = Vector3.zero;
            sunSurface.transform.localScale = Vector3.one;
            sunAudio.transform.Find("SurfaceAudio_Sun").GetComponent<AudioSource>().maxDistance = starModule.Size * 2f;
            sunAudio.name = "Audio_Star";

            var sunAtmosphere = GameObject.Instantiate(GameObject.Find("Sun_Body/Atmosphere_SUN"), sunGO.transform);
            sunSurface.transform.localPosition = Vector3.zero;
            sunSurface.transform.localScale = Vector3.one * 1.5f;
            sunAtmosphere.name = "Atmosphere_Star";

            var ambientLightGO = GameObject.Instantiate(GameObject.Find("Sun_Body/AmbientLight_SUN"), sunGO.transform);
            ambientLightGO.transform.localPosition = Vector3.zero;
            ambientLightGO.name = "AmbientLight_Star";

            PlanetaryFogController fog = sunAtmosphere.transform.Find("FogSphere").GetComponent<PlanetaryFogController>();
            TessellatedSphereRenderer surface = sunSurface.GetComponent<TessellatedSphereRenderer>();
            Light ambientLight = ambientLightGO.GetComponent<Light>();

            sunGO.transform.localPosition = Vector3.zero;
            sunGO.transform.localScale = starModule.Size * Vector3.one;
        }
    }
}
