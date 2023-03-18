using HarmonyLib;

namespace NewHorizons.Patches.VolumePatches
{
    [HarmonyPatch(typeof(VanishVolume))]
    public static class VanishVolumePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VanishVolume.Shrink))]
        public static bool VanishVolume_Shrink(VanishVolume __instance, OWRigidbody bodyToShrink)
        {
            return __instance != null && __instance.transform != null && __instance._shrinkingBodies != null && __instance._shrinkingBodyLocationData != null && bodyToShrink != null;
        }
    }
}
