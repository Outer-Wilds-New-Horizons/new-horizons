using HarmonyLib;
using NewHorizons.Components;

namespace NewHorizons.Patches;

[HarmonyPatch]
public static class DeathManagerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DeathManager), nameof(DeathManager.FinishDeathSequence))]
    public static void DeathManager_FinishDeathSequence()
    {
        NHGameOverManager.Instance.TryHijackDeathSequence();
    }
}
