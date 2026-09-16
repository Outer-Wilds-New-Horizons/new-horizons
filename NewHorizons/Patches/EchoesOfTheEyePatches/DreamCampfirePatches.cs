using HarmonyLib;
using NewHorizons.Components;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(DreamCampfire))]
    public static class DreamCampfirePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(DreamCampfire.OnEnterDreamWorld))]
        public static void DreamCampfire_OnEnterDreamWorld(DreamCampfire __instance)
        {
            EntrywayHandler.RemovePlayerFromTriggerVolumes(__instance.GetComponent<EntrywayVolumeHelper>());
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(DreamCampfire.OnExitDreamWorld))]
        public static void DreamCampfire_OnExitDreamWorld(DreamCampfire __instance)
        {
            EntrywayHandler.AddPlayerToTriggerVolumes(__instance.GetComponent<EntrywayVolumeHelper>());
        }
    }
}
