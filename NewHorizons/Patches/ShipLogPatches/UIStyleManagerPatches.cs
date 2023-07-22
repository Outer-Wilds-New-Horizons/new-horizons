using HarmonyLib;
using NewHorizons.Builder.ShipLog;
using UnityEngine;

namespace NewHorizons.Patches.ShipLogPatches
{
    [HarmonyPatch(typeof(UIStyleManager))]
    public static class UIStyleManagerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStyleManager.GetCuriosityColor))]
        public static bool UIStyleManager_GetCuriosityColor(UIStyleManager __instance, CuriosityName curiosityName, bool highlight, ref Color __result)
        {
            if ((int)curiosityName < 7)
            {
                return true;
            }
            else
            {
                __result = RumorModeBuilder.GetCuriosityColor(curiosityName, highlight, __instance._neutralColor, __instance._neutralHighlight);
                return false;
            }
        }
    }
}
