using HarmonyLib;
using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Patches.ShipLogPatches
{
    [HarmonyPatch(typeof(ShipLogEntryLocation))]
    public static class ShipLogEntryLocationPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogEntryLocation.OnValidate))]
        public static bool ShipLogEntryLocation_OnValidate(ShipLogEntryLocation __instance)
        {
            // This part is unchanged
            if (!__instance._entryID.Equals(string.Empty) && !__instance.gameObject.name.Equals(__instance._entryID))
            {
                __instance.gameObject.name = __instance._entryID;
            }

            // Base method checks if its on the Ringworld to see if it can be cloaked, we wanna check for a cloak field controller instead
            var cloak = __instance.GetComponentInChildren<CloakFieldController>();
            __instance._isWithinCloakField = cloak != null;
            return false;
        }
    }
}
