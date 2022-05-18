using NewHorizons.External.Modules;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class SunOverrideBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmo, float surfaceSize)
        {
            GameObject overrideGO = new GameObject("SunOverride");
            overrideGO.SetActive(false);
            overrideGO.transform.parent = sector?.transform ?? planetGO.transform;

            GiantsDeepSunOverrideVolume GDSOV = overrideGO.AddComponent<GiantsDeepSunOverrideVolume>();
            GDSOV._sector = sector;
            GDSOV._cloudsOuterRadius = atmo.Size;
            GDSOV._cloudsInnerRadius = atmo.Size * 0.9f;
            GDSOV._waterOuterRadius = surfaceSize;
            GDSOV._waterInnerRadius = 0f;

            overrideGO.transform.position = planetGO.transform.position;
            overrideGO.SetActive(true);
        }
    }
}
