using HarmonyLib;

namespace NewHorizons.Patches.ShipLogPatches
{
    [HarmonyPatch(typeof(SlideCollectionContainer))]
    public static class SlideCollectionContainerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.Initialize))]
        public static bool SlideCollectionContainer_Initialize(SlideCollectionContainer __instance, string id)
        {
            if (__instance._initialized)
                return false;
            __instance.SetupReadFlags();
            __instance.RegisterPerSlideCompletion();
            if (__instance.streamingTexturesAvailable)
                __instance.SetupStreaming();
            __instance.BuildMusicRangesIndex();
            __instance._changeSlidesAllowed = true;
            __instance._initialized = true;
            __instance._slideCollection.isVision = __instance._owningItem == null;
            foreach (var factID in __instance._playWithShipLogFacts)
            {
                var fact = Locator.GetShipLogManager().GetFact(factID);
                fact?.RegisterSlideCollection(__instance._slideCollection);
            }
            return false;
        }
    }
}