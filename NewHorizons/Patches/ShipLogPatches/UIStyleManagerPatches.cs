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
        public static bool UIStyleManager_GetCuriosityColor(UIStyleManager __instance, CuriosityName __0, bool __1, ref Color __result)
        {
            if ((int)__0 < 7)
            {
                return true;
            }
            else
            {
                __result = RumorModeBuilder.GetCuriosityColor(__0, __1, __instance._neutralColor, __instance._neutralHighlight);
                return false;
            }
        }
    }
}
