using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Tools
{
    public static class TranslationPatches
    {
        public static void Apply()
        {
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ReferenceFrame>(nameof(ReferenceFrame.GetHUDDisplayName), typeof(TranslationPatches), nameof(TranslationPatches.GetHUDDisplayName));

            var canvasMapMarkerInit = typeof(CanvasMapMarker).GetMethod(nameof(CanvasMapMarker.Init), new Type[] { typeof(Canvas), typeof(Transform), typeof(string) });
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix(canvasMapMarkerInit, typeof(TranslationPatches), nameof(TranslationPatches.OnCanvasMapMarkerInit));

            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<CanvasMapMarker>(nameof(CanvasMapMarker.SetLabel), typeof(TranslationPatches), nameof(TranslationPatches.OnCanvasMapMarkerSetLabel));

        }

        public static bool GetHUDDisplayName(ReferenceFrame __instance, ref string __result)
        {
            var ao = __instance.GetAstroObject();
            if (ao != null && ao.GetAstroObjectName() == AstroObject.Name.CustomString)
            {
                __result = TranslationHandler.GetTranslation(ao.GetCustomName(), TranslationHandler.TextType.UI);
                return false;
            }
            return true;
        }

        public static void OnCanvasMapMarkerInit(CanvasMapMarker __instance)
        {
            __instance._label = TranslationHandler.GetTranslation(__instance._label, TranslationHandler.TextType.UI);
        }

        public static void OnCanvasMapMarkerSetLabel(CanvasMapMarker __instance)
        {
            __instance._label = TranslationHandler.GetTranslation(__instance._label, TranslationHandler.TextType.UI);
        }
    }
}
