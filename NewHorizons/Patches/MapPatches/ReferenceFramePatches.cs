using HarmonyLib;
using NewHorizons.Components.EyeOfTheUniverse;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.MapPatches
{
    [HarmonyPatch(typeof(ReferenceFrame))]
    public static class ReferenceFramePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ReferenceFrame.GetHUDDisplayName))]
        public static void ReferenceFrame_GetHUDDisplayName(ReferenceFrame __instance, ref string __result)
        {
            var ao = __instance.GetAstroObject();

            if (ao is EyeAstroObject eyeao && !eyeao.HideDisplayName)
            {
                var name = eyeao.GetAstroObjectName();

                if (name == AstroObject.Name.Eye)
                {
                    __result = UITextLibrary.GetString(UITextType.LocationEye);
                }
                else
                {
                    var customName = eyeao.GetCustomName();

                    if (!string.IsNullOrWhiteSpace(customName))
                    {
                        __result = TranslationHandler.GetTranslation(customName, TranslationHandler.TextType.UI, false);
                    }
                }
            }
            else if (ao is NHAstroObject nhao && !nhao.isVanilla && !nhao.HideDisplayName)
            {
                var customName = nhao.GetCustomName();

                if (!string.IsNullOrWhiteSpace(customName))
                {
                    __result = TranslationHandler.GetTranslation(customName, TranslationHandler.TextType.UI, false);
                }
            }
        }
    }
}
