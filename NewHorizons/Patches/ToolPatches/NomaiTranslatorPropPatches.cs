using HarmonyLib;
using NewHorizons.Components;
using System.Linq;

namespace NewHorizons.Patches.ToolPatches;

[HarmonyPatch(typeof(ProbeLauncher))]
public static class NomaiTranslatorPropPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.DisplayTextNode))]
    public static bool NomaiTranslatorProp_DisplayTextNode(NomaiTranslatorProp __instance)
    {
        // Adapted from Forgotten Castaways. Thanks coderCleric! I love stealing!
        bool illegible = __instance._scanBeams.FirstOrDefault()?._nomaiTextLine?.gameObject?.GetComponent<ConditionalNomaiTextTranslatable>()?.IsIllegible() ?? false;

        // Hide the text
        if (illegible)
        {
            __instance._textField.text = UITextLibrary.GetString(UITextType.TranslatorUntranslatableWarning);
            return false;
        }

        // Otherwise, run normally
        return true;
    }
}
