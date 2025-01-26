using HarmonyLib;

namespace NewHorizons.Components.EOTE;

[HarmonyPatch(typeof(SlideCollectionContainer))]
public class NHSlideCollectionContainer : SlideCollectionContainer
{
    public string[] conditionsToSet;
    public string[] persistentConditionsToSet;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.Initialize))]
    public static bool SlideCollectionContainer_Initialize(SlideCollectionContainer __instance)
    {
        if (__instance is NHSlideCollectionContainer)
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
            if (__instance._playWithShipLogFacts != null)
            {
                foreach (var factID in __instance._playWithShipLogFacts)
                {
                    var fact = Locator.GetShipLogManager().GetFact(factID);
                    fact?.RegisterSlideCollection(__instance._slideCollection);
                }
            }
            return false;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.SetReadFlag))]
    public static void SlideCollectionContainer_SetReadFlag(SlideCollectionContainer __instance)
    {
        if (__instance is NHSlideCollectionContainer container)
        {
            if (container._unreadSlideIndices.Count == 0)
            {
                if (container.conditionsToSet != null)
                {
                    foreach (var condition in container.conditionsToSet)
                    {
                        DialogueConditionManager.SharedInstance.SetConditionState(condition, true);
                    }
                }
                if (container.persistentConditionsToSet != null)
                {
                    foreach (var condition in container.persistentConditionsToSet)
                    {
                        PlayerData.SetPersistentCondition(condition, true);
                    }
                }
            }
        }
    }
}
