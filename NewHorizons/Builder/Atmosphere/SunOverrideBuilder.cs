#region

using NewHorizons.External.Modules;
using UnityEngine;

#endregion

namespace NewHorizons.Builder.Atmosphere
{
    public static class SunOverrideBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmo, float surfaceSize)
        {
            var overrideGO = new GameObject("SunOverride");
            overrideGO.SetActive(false);
            overrideGO.transform.parent = sector?.transform ?? planetGO.transform;

            var GDSOV = overrideGO.AddComponent<GiantsDeepSunOverrideVolume>();
            GDSOV._sector = sector;
            GDSOV._cloudsOuterRadius = atmo.size;
            GDSOV._cloudsInnerRadius = atmo.size * 0.9f;
            GDSOV._waterOuterRadius = surfaceSize;
            GDSOV._waterInnerRadius = 0f;

            overrideGO.transform.position = planetGO.transform.position;
            overrideGO.SetActive(true);
        }
    }
}