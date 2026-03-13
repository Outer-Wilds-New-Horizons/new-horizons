using HarmonyLib;
using NewHorizons.Components.Volumes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Patches.VolumePatches
{

    [HarmonyPatch(typeof(RepairReceiver))]
    public static class RepairReceiverPatches
    {
        // We can't actually override these methods so we patch the base class methods to invoke the subclass methods dynamically

        [HarmonyPostfix, HarmonyPatch(nameof(RepairReceiver.IsRepairable))]
        public static void IsRepairable(RepairReceiver __instance, ref bool __result)
        {
            if (__instance is NHRepairReceiver r)
            {
                __result = r.IsRepairable();
            }
        }

        [HarmonyPostfix, HarmonyPatch(nameof(RepairReceiver.RepairTick))]
        public static void RepairTick(RepairReceiver __instance)
        {
            if (__instance is NHRepairReceiver r)
            {
                r.RepairTick();
            }
        }

        [HarmonyPostfix, HarmonyPatch(nameof(RepairReceiver.IsDamaged))]
        public static void IsDamaged(RepairReceiver __instance, ref bool __result)
        {
            if (__instance is NHRepairReceiver r)
            {
                __result = r.IsDamaged();
            }
        }

        [HarmonyPostfix, HarmonyPatch(nameof(RepairReceiver.GetRepairableName))]
        public static void GetRepairableName(RepairReceiver __instance, ref UITextType __result)
        {
            if (__instance is NHRepairReceiver r)
            {
                __result = r.GetRepairableName();
            }
        }

        [HarmonyPostfix, HarmonyPatch(nameof(RepairReceiver.GetRepairFraction))]
        public static void GetRepairFraction(RepairReceiver __instance, ref float __result)
        {
            if (__instance is NHRepairReceiver r)
            {
                __result = r.GetRepairFraction();
            }
        }
    }
}
