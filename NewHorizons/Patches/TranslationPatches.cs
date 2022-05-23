using HarmonyLib;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;
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

            if (ao is NHAstroObject)
            {
                if ((ao as NHAstroObject).HideDisplayName) __result = "";
                else __result = TranslationHandler.GetTranslation(ao.GetCustomName(), TranslationHandler.TextType.UI);
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CanvasMapMarker), nameof(CanvasMapMarker.Init), typeof(Canvas), typeof(Transform),
            typeof(string))]
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