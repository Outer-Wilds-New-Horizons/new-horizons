using NewHorizons.Handlers;
using UnityEngine;

namespace NewHorizons.Components;

public class ConditionalNomaiTextTranslatable : MonoBehaviour
{
    public string legiblePersistentCondition;
    public string customLanguageName;

    private string _translatedPrompt;

    public void Start()
    {
        if (string.IsNullOrEmpty(customLanguageName))
        {
            _translatedPrompt = TranslationHandler.GetTranslation("UNKNOWN_LANGUAGE_UNTRANSLATED", TranslationHandler.TextType.OTHER);
        }
        else
        {
            var name = TranslationHandler.GetTranslation(customLanguageName, TranslationHandler.TextType.OTHER);
            _translatedPrompt = TranslationHandler.GetTranslation("KNOWN_LANGUAGE_UNTRANSLATED", TranslationHandler.TextType.OTHER).Replace("{0}", name);
        }
    }

    public bool IsIllegible() => !(string.IsNullOrEmpty(legiblePersistentCondition) || PlayerData.GetPersistentCondition(legiblePersistentCondition));

    public string GetUntranslatedPrompt() => _translatedPrompt;
}
