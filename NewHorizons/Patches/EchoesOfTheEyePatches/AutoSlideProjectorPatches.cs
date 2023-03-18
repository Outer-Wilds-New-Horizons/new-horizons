using HarmonyLib;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(AutoSlideProjector))]
    public static class AutoSlideProjectorPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(AutoSlideProjector.Play))]
        public static void AutoSlideProjector_Play(AutoSlideProjector __instance)
        {
            __instance._slideCollectionItem.enabled = true;
        }
    }
}