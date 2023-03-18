using HarmonyLib;
using UnityEngine;

namespace NewHorizons.Patches.SunPatches
{
    [HarmonyPatch(typeof(SunSurfaceAudioController))]
    public static class SunSurfaceAudioControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SunSurfaceAudioController.Update))]
        public static bool SunSurfaceAudioController_Update(SunSurfaceAudioController __instance)
        {
            if (__instance._sunController != null) return true;

            var surfaceRadius = __instance.transform.parent.parent.localScale.magnitude;
            float value = Mathf.Max(0f, Vector3.Distance(Locator.GetPlayerCamera().transform.position, __instance.transform.position) - surfaceRadius);
            float num = Mathf.InverseLerp(1600f, 100f, value);
            __instance._audioSource.SetLocalVolume(num * num * __instance._fade);
            return false;
        }
    }
}
