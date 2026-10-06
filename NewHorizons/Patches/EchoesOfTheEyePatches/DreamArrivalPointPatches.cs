using HarmonyLib;
using NewHorizons.Components;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(DreamArrivalPoint))]
    public static class DreamArrivalPointPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(DreamArrivalPoint.OnEnterDreamWorld))]
        public static void DreamArrivalPoint_OnEnterDreamWorld(DreamArrivalPoint __instance)
        {
            EntrywayHandler.AddPlayerToTriggerVolumes(__instance.GetComponent<EntrywayVolumeHelper>());
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(DreamArrivalPoint.OnExitDreamWorld))]
        public static void DreamArrivalPoint_OnExitDreamWorld(DreamArrivalPoint __instance)
        {
            EntrywayHandler.AddPlayerToTriggerVolumes(__instance.GetComponent<EntrywayVolumeHelper>());
        }
    }
}
