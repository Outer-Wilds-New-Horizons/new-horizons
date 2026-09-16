using HarmonyLib;
using NewHorizons.Components;
using NewHorizons.Handlers;

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

        // Each of these methods are called before teleporting an object to the linked white hole. Needs to be a prefix so trigger volumes are removed before the white hole potentially adds the same volumes back
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BlackHoleVolume.Vanish))]
        [HarmonyPatch(nameof(BlackHoleVolume.VanishPlayer))]
        [HarmonyPatch(nameof(BlackHoleVolume.VanishShip))]
        [HarmonyPatch(nameof(BlackHoleVolume.VanishShipCockpit))]
        [HarmonyPatch(nameof(BlackHoleVolume.VanishProbe))]
        [HarmonyPatch(nameof(BlackHoleVolume.VanishNomaiShuttle))]
        public static void BlackHoleVolume_Vanish(BlackHoleVolume __instance, OWRigidbody __0, RelativeLocationData __1)
        {
            if (__instance._whiteHole != null)
            {
                EntrywayHandler.RemoveBodyFromTriggerVolumes(__0, __instance.GetComponent<EntrywayVolumeHelper>());
            }
        }
    }
}
