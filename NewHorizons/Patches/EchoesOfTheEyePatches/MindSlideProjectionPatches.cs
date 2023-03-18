using HarmonyLib;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(MindSlideProjector))]
    public static class MindSlideProjectionPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MindSlideProjector), nameof(MindSlideProjector.SetMindSlideCollection))]
        private static bool MindSlideProjector_SetMindSlideCollection(MindSlideProjector __instance, MindSlideCollection mindSlideCollection)
        {
            if (mindSlideCollection == null) return false;

            if (__instance._mindSlideCollection == mindSlideCollection) return false;

            // Original method didn't check if old _slideCollectionItem was null.
            if (__instance._slideCollectionItem != null)
            {
                __instance._slideCollectionItem.onSlideTextureUpdated -= __instance.OnSlideTextureUpdated;
                __instance._slideCollectionItem.onPlayBeatAudio -= __instance.OnPlayBeatAudio;
            }

            __instance._mindSlideCollection = mindSlideCollection;
            __instance._defaultSlideDuration = mindSlideCollection.defaultSlideDuration;

            __instance._slideCollectionItem = mindSlideCollection.slideCollectionContainer;
            __instance._slideCollectionItem.onSlideTextureUpdated += __instance.OnSlideTextureUpdated;
            __instance._slideCollectionItem.onPlayBeatAudio += __instance.OnPlayBeatAudio;
            __instance._slideCollectionItem.Initialize();
            __instance._slideCollectionItem.enabled = false;

            return false;
        }
    }
}
