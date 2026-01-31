using HarmonyLib;
using NewHorizons.Components;
using System.Linq;

namespace NewHorizons.Patches.ToolPatches;

[HarmonyPatch(typeof(NomaiTranslatorProp))]
public static class NomaiTranslatorPropPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.DisplayTextNode))]
    public static bool NomaiTranslatorProp_DisplayTextNode_Prefix(NomaiTranslatorProp __instance)
    {
        // Adapted from Forgotten Castaways. Thanks coderCleric! I love stealing!
        var component = __instance._scanBeams.FirstOrDefault()?._nomaiTextLine?.gameObject?.GetComponent<ConditionalNomaiTextTranslatable>();

        // Hide the text
        if (component != null && component.IsIllegible())
        {
            __instance._textField.text = UITextLibrary.GetString(UITextType.TranslatorUntranslatableWarning);
            return false;
        }

        // Otherwise, run normally
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.DisplayTextNode))]
    public static void NomaiTranslatorProp_DisplayTextNode_Postfix(NomaiTranslatorProp __instance)
    {
        // Adapted from Forgotten Castaways. Thanks coderCleric! I love stealing!
        var component = __instance._scanBeams.FirstOrDefault()?._nomaiTextLine?.gameObject?.GetComponent<ConditionalNomaiTextTranslatable>();

        if (component != null && !component.IsIllegible() && !__instance._nomaiTextComponent.IsTranslated(__instance._currentTextID) && __instance._translationTimeElapsed == 0f)
        {
            __instance._textField.text = component.GetUntranslatedPrompt();
        }
    }
}
