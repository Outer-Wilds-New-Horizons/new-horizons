using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.ShipLogPatches
{
    [HarmonyPatch(typeof(ShipLogFact))]
    public static class ShipLogFactPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogFact.GetText))]
        public static bool ShipLogFact_GetText(ShipLogFact __instance, ref string __result)
        {
            if (ShipLogHandler.IsModdedFact(__instance.GetID()))
            {
                __result = TranslationHandler.GetTranslation(__instance._text, TranslationHandler.TextType.SHIPLOG);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
