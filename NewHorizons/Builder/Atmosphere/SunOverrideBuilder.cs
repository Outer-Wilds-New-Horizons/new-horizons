using NewHorizons.External;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Atmosphere
{
    static class SunOverrideBuilder
    {
        public static void Make(GameObject body, Sector sector, float surfaceSize, AtmosphereModule atmo)
        {
            GameObject overrideGO = new GameObject("SunOverride");
            overrideGO.SetActive(false);
            overrideGO.transform.parent = body.transform;

            GiantsDeepSunOverrideVolume GDSOV = overrideGO.AddComponent<GiantsDeepSunOverrideVolume>();
            GDSOV._sector = sector;
            GDSOV._cloudsOuterRadius = atmo.Size;
            GDSOV._cloudsInnerRadius = atmo.Size * 0.9f;
            GDSOV._waterOuterRadius = surfaceSize;
            GDSOV._waterInnerRadius = 0f;

            overrideGO.transform.localPosition = Vector3.zero;
            overrideGO.SetActive(true);
        }
    }
}
