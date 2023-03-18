using HarmonyLib;
using NewHorizons.Builder.ShipLog;
using NewHorizons.Components.ShipLog;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using UnityEngine;

namespace NewHorizons.Patches.ShipLogPatches
{
    [HarmonyPatch(typeof(ShipLogAstroObject))]
    public static class ShipLogAstroObjectPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogAstroObject.GetName))]
        public static bool ShipLogAstroObject_GetName(ShipLogAstroObject __instance, ref string __result)
        {
            if (ShipLogHandler.IsVanillaAstroID(__instance.GetID()))
            {
                return true;
            }
            else
            {
                __result = MapModeBuilder.GetAstroBodyShipLogName(__instance.GetID());
                return false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ShipLogAstroObject.UpdateState))]
        public static void ShipLogAstroObject_UpdateState(ShipLogAstroObject __instance)
        {
            Transform detailsParent = __instance.transform.Find("Details");
            if (detailsParent != null)
            {
                foreach (GameObject child in detailsParent.gameObject.GetAllChildren())
                {
                    if (child.TryGetComponent(typeof(ShipLogDetail), out Component detail))
                    {
                        (detail as ShipLogDetail)?.UpdateState(__instance._state);
                    }
                }
            }

            Transform lineObject = __instance.transform.Find("Line_ShipLog");
            if (lineObject != null)
            {
                ShipLogDetail lineDetail = lineObject.gameObject.GetComponent<ShipLogDetail>();
                lineDetail.UpdateState(__instance._state);
            }
        }
    }
}
