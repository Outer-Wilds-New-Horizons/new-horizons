using HarmonyLib;

namespace NewHorizons.Patches.VolumePatches
{
    [HarmonyPatch(typeof(BlackHoleVolume))]
    public static class BlackHoleVolumePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BlackHoleVolume.Start))]
        public static bool BlackHoleVolume_Start(BlackHoleVolume __instance)
        {
            return __instance._whiteHole == null;
        }
    }
}
