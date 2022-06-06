using HarmonyLib;
using UnityEngine;
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
                Vector3 position = __instance.transform.position;
                float w = 2000f;
                if (__instance._sunController != null)
                {
                    w = (__instance._sunController.HasSupernovaStarted() ? __instance._sunController.GetSupernovaRadius() : __instance._sunController.GetSurfaceRadius());
                }
                float range = __instance.sunLight.range;
                Color color = (__instance._sunLightController != null) ? __instance._sunLightController.sunColor : __instance.sunLight.color;
                float w2 = (__instance._sunLightController != null) ? __instance._sunLightController.sunIntensity : __instance.sunLight.intensity;
                Shader.SetGlobalVector(__instance._propID_SunPosition, new Vector4(position.x, position.y, position.z, w));
                Shader.SetGlobalVector(__instance._propID_OWSunPositionRange, new Vector4(position.x, position.y, position.z, 1f / (range * range)));
                Shader.SetGlobalVector(__instance._propID_OWSunColorIntensity, new Vector4(color.r, color.g, color.b, w2));
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SunSurfaceAudioController), nameof(SunSurfaceAudioController.Update))]
        public static bool SunSurfaceAudioController_Update(SunSurfaceAudioController __instance)
        {
            if (__instance._sunController != null) return true;

            var surfaceRadius = __instance.transform.parent.parent.localScale.magnitude;
            float value = Mathf.Max(0f, Vector3.Distance(Locator.GetPlayerCamera().transform.position, __instance.transform.position) - surfaceRadius);
            float num = Mathf.InverseLerp(1600f, 100f, value);
            __instance._audioSource.SetLocalVolume(num * num * __instance._fade);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SunProxyEffectController), nameof(SunProxyEffectController.UpdateScales))]
        public static bool SunProxyEffectController_UpdateScales(SunProxyEffectController __instance)
        {
            return __instance != null && __instance._surface != null && __instance._fog != null && __instance._fogMaterial != null && __instance._solarFlareEmitter != null && __instance._atmosphere != null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SunProxyEffectController), nameof(SunProxyEffectController.UpdateAtmosphereRadii))]
        public static bool SunProxyEffectController_UpdateAtmosphereRadii(SunProxyEffectController __instance)
        {
            return __instance != null && __instance.transform != null && __instance.transform.parent != null && __instance._atmosphereMaterial != null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VanishVolume), nameof(VanishVolume.Shrink))]
        public static bool VanishVolume_Shrink(VanishVolume __instance, OWRigidbody bodyToShrink)
        {
            return __instance != null && __instance.transform != null && __instance._shrinkingBodies != null && __instance._shrinkingBodyLocationData != null && bodyToShrink != null;
        }
    }
}
