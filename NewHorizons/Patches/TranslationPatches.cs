using HarmonyLib;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;
using System;
using UnityEngine;
namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class TranslationPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ReferenceFrame), nameof(ReferenceFrame.GetHUDDisplayName))]
        public static bool ReferenceFrame_GetHUDDisplayName(ReferenceFrame __instance, ref string __result)
        {
            var ao = __instance.GetAstroObject();

            if (ao == null || ao._name != AstroObject.Name.CustomString) return true;

            __result = string.Empty;

            if (ao is NHAstroObject nhao && nhao.HideDisplayName) return false;

            var customName = ao.GetCustomName();

            if (!string.IsNullOrWhiteSpace(customName))
            {
                __result = TranslationHandler.GetTranslation(customName, TranslationHandler.TextType.UI);
            }

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CanvasMapMarker), nameof(CanvasMapMarker.Init), new Type[] { typeof(Canvas), typeof(Transform), typeof(string) })]
        public static void CanvasMapMarker_Init(CanvasMapMarker __instance)
        {
            __instance._label = TranslationHandler.GetTranslation(__instance._label, TranslationHandler.TextType.UI);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CanvasMapMarker), nameof(CanvasMapMarker.SetLabel))]
        public static void CanvasMapMarker_SetLabel(CanvasMapMarker __instance)
        {
            __instance._label = TranslationHandler.GetTranslation(__instance._label, TranslationHandler.TextType.UI);
        }
    }
}
