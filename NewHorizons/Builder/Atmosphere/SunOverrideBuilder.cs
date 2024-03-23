using NewHorizons.External.Modules;
using NewHorizons.External.Modules.VariableSize;
using UnityEngine;

namespace NewHorizons.Builder.Atmosphere
{
    public static class SunOverrideBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmo, WaterModule water, float surfaceSize)
        {
            GameObject overrideGO = new GameObject("SunOverride");
            overrideGO.SetActive(false);
            overrideGO.transform.parent = sector?.transform ?? planetGO.transform;

            if (water != null)
            {
                var GDSOV = overrideGO.AddComponent<GiantsDeepSunOverrideVolume>();
                GDSOV._sector = sector;
                GDSOV._cloudsOuterRadius = atmo.clouds.outerCloudRadius;
                GDSOV._cloudsInnerRadius = atmo.clouds.innerCloudRadius;
                GDSOV._waterOuterRadius = water.size;
                GDSOV._waterInnerRadius = 0f;
            }
            else
            {
                var sunOverride = overrideGO.AddComponent<SunOverrideVolume>();
                sunOverride._sector = sector;
                
                sunOverride._overrideColor = true;
                sunOverride._color = Color.black;

                sunOverride._overrideIntensity = true;
                sunOverride._intensity = 0f;

                sunOverride._overrideShadowStrength = true;
                sunOverride._shadowStrength = 1f;

                sunOverride.shape = SimpleVolume.Shape.Sphere;
                sunOverride.height = 2;
                sunOverride.radius = atmo.clouds.innerCloudRadius;
            }

            overrideGO.transform.position = planetGO.transform.position;
            overrideGO.SetActive(true);
        }
    }
}
