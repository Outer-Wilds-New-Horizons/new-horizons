using HarmonyLib;
using UnityEngine;
namespace NewHorizons.Patches.SunPatches
{
    [HarmonyPatch(typeof(SunLightParamUpdater))]
    public static class SunLightParamUpdaterPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SunLightParamUpdater), nameof(SunLightParamUpdater.LateUpdate))]
        public static bool SunLightParamUpdater_LateUpdate(SunLightParamUpdater __instance)
        {
            if (__instance.sunLight)
            {
                Vector3 position = __instance.transform.position;
                float w = 2000f;
                if (__instance._sunController != null)
                {
                    w = __instance._sunController.HasSupernovaStarted() ? __instance._sunController.GetSupernovaRadius() : __instance._sunController.GetSurfaceRadius();
                }
                float range = __instance.sunLight.range;
                Color color = __instance._sunLightController != null ? __instance._sunLightController.sunColor : __instance.sunLight.color;
                float w2 = __instance._sunLightController != null ? __instance._sunLightController.sunIntensity : __instance.sunLight.intensity;
                Shader.SetGlobalVector(__instance._propID_SunPosition, new Vector4(position.x, position.y, position.z, w));
                Shader.SetGlobalVector(__instance._propID_OWSunPositionRange, new Vector4(position.x, position.y, position.z, 1f / (range * range)));
                Shader.SetGlobalVector(__instance._propID_OWSunColorIntensity, new Vector4(color.r, color.g, color.b, w2));
            }

            return false;
        }
    }
}
