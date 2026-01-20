using HarmonyLib;
using NewHorizons.Components.Props;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch(typeof(NomaiTextLine))]
    public static class TextLinePatches
    {
        [HarmonyPrefix, HarmonyPatch(nameof(NomaiTextLine.DetermineTextLineColor))]
        public static bool DetermineTextLineColor(NomaiTextLine __instance, NomaiTextLine.VisualState state, ref Color __result)
        {
            if (__instance.TryGetComponent(out NHTranslatorTextLineColorizer t))
            {
                __result = t.DetermineTextLineColor(state);
                return false;
            }
            return true;
        }
    }
}
