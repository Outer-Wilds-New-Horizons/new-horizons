using HarmonyLib;
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
            if (__instance.GetAstroObject() is NHAstroObject nhao && !nhao.isVanilla && !nhao.HideDisplayName)
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
