using HarmonyLib;
using NewHorizons.Handlers;
using System;
using UnityEngine;

namespace NewHorizons.Patches.MapPatches
{
    [HarmonyPatch(typeof(CanvasMapMarker))]
    public static class CanvasMapMarkerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CanvasMapMarker.Init), new Type[] { typeof(Canvas), typeof(Transform), typeof(string) })]
        [HarmonyPatch(nameof(CanvasMapMarker.SetLabel))]
        public static void CanvasMapMarker_LocalizeLabel(CanvasMapMarker __instance)
        {
            __instance._label = TranslationHandler.GetTranslation(__instance._label, TranslationHandler.TextType.UI);
        }
    }
}
