using Marshmallow.External;
using OWML.ModHelper.Events;
using UnityEngine;
using Logger = Marshmallow.Utility.Logger;

namespace Marshmallow.Atmosphere
{
    static class SunOverrideBuilder
    {
        public static void Make(GameObject body, Sector sector, IPlanetConfig config)
        {
            GameObject overrideGO = new GameObject();
            overrideGO.SetActive(false);
            overrideGO.transform.parent = body.transform;

            GiantsDeepSunOverrideVolume GDSOV = overrideGO.AddComponent<GiantsDeepSunOverrideVolume>();
            GDSOV.SetValue("_sector", sector);
            GDSOV.SetValue("_cloudsOuterRadius", config.TopCloudSize / 2);
            GDSOV.SetValue("_cloudsInnerRadius", config.BottomCloudSize / 2);
            GDSOV.SetValue("_waterOuterRadius", config.WaterSize / 2);
            GDSOV.SetValue("_waterInnerRadius", 402.5f);

            overrideGO.SetActive(true);
            Logger.Log("Finished building sun override.", Logger.LogType.Log);
        }
    }
}
