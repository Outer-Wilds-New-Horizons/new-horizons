#region

using HarmonyLib;
using UnityEngine;

#endregion

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class SunPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SunLightParamUpdater), nameof(SunLightParamUpdater.LateUpdate))]
        public static bool SunLightParamUpdater_LateUpdate(SunLightParamUpdater __instance)
        {
            if (__instance.sunLight)
            {
                var position = __instance.transform.position;
                var w = 2000f;
                if (__instance._sunController != null)
                    w = __instance._sunController.HasSupernovaStarted()
                        ? __instance._sunController.GetSupernovaRadius()
                        : __instance._sunController.GetSurfaceRadius();
                var range = __instance.sunLight.range;
                var color = __instance._sunLightController != null
                    ? __instance._sunLightController.sunColor
                    : __instance.sunLight.color;
                var w2 = __instance._sunLightController != null
                    ? __instance._sunLightController.sunIntensity
                    : __instance.sunLight.intensity;
                Shader.SetGlobalVector(__instance._propID_SunPosition,
                    new Vector4(position.x, position.y, position.z, w));
                Shader.SetGlobalVector(__instance._propID_OWSunPositionRange,
                    new Vector4(position.x, position.y, position.z, 1f / (range * range)));
                Shader.SetGlobalVector(__instance._propID_OWSunColorIntensity,
                    new Vector4(color.r, color.g, color.b, w2));
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SunSurfaceAudioController), nameof(SunSurfaceAudioController.Update))]
        public static bool SunSurfaceAudioController_Update(SunSurfaceAudioController __instance)
        {
            if (__instance._sunController != null) return true;

            var surfaceRadius = __instance.transform.parent.parent.localScale.magnitude;
            var value = Mathf.Max(0f,
                Vector3.Distance(Locator.GetPlayerCamera().transform.position, __instance.transform.position) -
                surfaceRadius);
            var num = Mathf.InverseLerp(1600f, 100f, value);
            __instance._audioSource.SetLocalVolume(num * num * __instance._fade);
            return false;
        }
    }
}