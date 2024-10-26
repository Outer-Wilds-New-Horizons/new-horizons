using HarmonyLib;

namespace NewHorizons.Components.EOTE;

[HarmonyPatch(typeof(SlideCollectionContainer))]
public class NHSlideCollectionContainer : SlideCollectionContainer
{
    public string[] conditionsToSet;
    public string[] persistentConditionsToSet;

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
