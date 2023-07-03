using HarmonyLib;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.MapPatches
{
    [HarmonyPatch(typeof(ReferenceFrame))]
    public static class ReferenceFramePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ReferenceFrame.GetHUDDisplayName))]
        public static bool ReferenceFrame_GetHUDDisplayName(ReferenceFrame __instance, ref string __result)
        {
            var ao = __instance.GetAstroObject();

            if (ao == null) return true;

            if (ao._name
                is not AstroObject.Name.CustomString
                and (<= AstroObject.Name.Comet or >= AstroObject.Name.TimberMoon))
            {
                __result = AstroObject.AstroObjectNameToString(ao._name);
                return false;
            }

            __result = string.Empty;

            if (ao is NHAstroObject nhao && nhao.HideDisplayName) return false;

            var customName = ao.GetCustomName();

            if (!string.IsNullOrWhiteSpace(customName))
            {
                __result = TranslationHandler.GetTranslation(customName, TranslationHandler.TextType.UI, false);
            }

            return false;
        }
    }
}
