using NewHorizons.External.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class TranslationHandler
    {
        public static Dictionary<TextTranslation.Language, Dictionary<string, string>> ShipLogTranslationDictionary = new Dictionary<TextTranslation.Language, Dictionary<string, string>>();
        public static Dictionary<TextTranslation.Language, Dictionary<string, string>> DialogueTranslationDictionary = new Dictionary<TextTranslation.Language, Dictionary<string, string>>();

        public static void RegisterTranslation(TextTranslation.Language language, TranslationConfig config)
        {
            if (config.ShipLogDictionary != null && config.ShipLogDictionary.Count() > 0)
            {
                if (!ShipLogTranslationDictionary.ContainsKey(language)) ShipLogTranslationDictionary.Add(language, new Dictionary<string, string>());
                foreach (var originalKey in config.ShipLogDictionary.Keys)
                {
                    var key = originalKey.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");

                    if (!ShipLogTranslationDictionary[language].ContainsKey(key)) ShipLogTranslationDictionary[language].Add(key, config.ShipLogDictionary[originalKey]);
                    else Logger.LogError($"Duplicate ship log translation for {originalKey}");
                }
            }

            if (config.DialogueDictionary != null && config.DialogueDictionary.Count() > 0)
            {
                if (!DialogueTranslationDictionary.ContainsKey(language)) DialogueTranslationDictionary.Add(language, new Dictionary<string, string>());
                foreach (var originalKey in config.DialogueDictionary.Keys)
                {
                    var key = originalKey.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");

                    if (!DialogueTranslationDictionary[language].ContainsKey(key)) DialogueTranslationDictionary[language].Add(key, config.DialogueDictionary[originalKey]);
                    else Logger.LogError($"Duplicate dialogue translation for {originalKey}");
                }
            }
        }

        public static void AddDialogue(string rawText, params string[] rawPreText)
        {
            var key = string.Join(string.Empty, rawPreText) + rawText;
            var language = TextTranslation.Get().m_language;

            string text = rawText;
            if (DialogueTranslationDictionary.TryGetValue(language, out var dict) && dict.TryGetValue(rawText, out var translatedText)) text = translatedText;

            TextTranslation.Get().m_table.Insert(key, text);
        }

        public static void AddShipLog(string rawText, params string[] rawPreText)
        {
            var key = string.Join(string.Empty, rawPreText) + rawText;
            var language = TextTranslation.Get().m_language;

            string text = rawText;
            if (ShipLogTranslationDictionary.TryGetValue(language, out var dict) && dict.TryGetValue(rawText, out var translatedText)) text = translatedText;

            TextTranslation.Get().m_table.InsertShipLog(key, text);
        }
    }
}
