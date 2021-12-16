using NewHorizons.External;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Atmosphere
{
    static class SunOverrideBuilder
    {
        public static void Make(GameObject body, Sector sector, float surfaceSize, AtmosphereModule atmo)
        {
            GameObject overrideGO = new GameObject("SunOverride");
            overrideGO.SetActive(false);
            overrideGO.transform.parent = body.transform;

            GiantsDeepSunOverrideVolume GDSOV = overrideGO.AddComponent<GiantsDeepSunOverrideVolume>();
            GDSOV.SetValue("_sector", sector);
            GDSOV.SetValue("_cloudsOuterRadius", atmo.Size);
            GDSOV.SetValue("_cloudsInnerRadius", atmo.Size * 0.9f);
            GDSOV.SetValue("_waterOuterRadius", surfaceSize);
            GDSOV.SetValue("_waterInnerRadius", 0f);

            overrideGO.SetActive(true);
            Logger.Log("Finished building sun override.", Logger.LogType.Log);
        }
    }
}
