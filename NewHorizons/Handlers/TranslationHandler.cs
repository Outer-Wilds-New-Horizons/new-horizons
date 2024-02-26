using NewHorizons.External.Configs;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewHorizons.Handlers
{
    public static class TranslationHandler
    {
        private static Dictionary<TextTranslation.Language, Dictionary<string, string>> _shipLogTranslationDictionary = new();
        private static Dictionary<TextTranslation.Language, Dictionary<string, string>> _dialogueTranslationDictionary = new();
        private static Dictionary<TextTranslation.Language, Dictionary<string, string>> _uiTranslationDictionary = new();
        private static Dictionary<TextTranslation.Language, Dictionary<string, string>> _otherTranslationDictionary = new();

        public enum TextType
        {
            SHIPLOG,
            DIALOGUE,
            UI,
            OTHER
        }

        public static string GetTranslation(string text, TextType type) => GetTranslation(text, type, true);

        public static string GetTranslation(string text, TextType type, bool warn)
        {
            Dictionary<TextTranslation.Language, Dictionary<string, string>> dictionary;
            var language = TextTranslation.Get().m_language;

            switch (type)
            {
                case TextType.SHIPLOG:
                    dictionary = _shipLogTranslationDictionary;
                    break;
                case TextType.DIALOGUE:
                    dictionary = _dialogueTranslationDictionary;
                    break;
                case TextType.UI:
                    dictionary = _uiTranslationDictionary;
                    break;                     
                case TextType.OTHER:
                    dictionary = _otherTranslationDictionary;
                    break;
                default:
                    if (warn) NHLogger.LogVerbose($"Invalid TextType {type}");
                    return text;
            }

            // Get the translated text
            if (dictionary.TryGetValue(language, out var table))
                if (table.TryGetValue(text, out var translatedText))
                    return translatedText;

            if (warn) NHLogger.LogVerbose($"Defaulting to english for {text}");

            // Try to default to English
            if (dictionary.TryGetValue(TextTranslation.Language.ENGLISH, out var englishTable))
                if (englishTable.TryGetValue(text, out var englishText))
                    return englishText;

            if (warn) NHLogger.LogVerbose($"Defaulting to key for {text}");

            // Default to the key
            return text;
        }

        public static void RegisterTranslation(TextTranslation.Language language, TranslationConfig config)
        {
            if (config.ShipLogDictionary != null && config.ShipLogDictionary.Count > 0)
            {
                if (!_shipLogTranslationDictionary.ContainsKey(language)) _shipLogTranslationDictionary.Add(language, new Dictionary<string, string>());
                foreach (var originalKey in config.ShipLogDictionary.Keys)
                {
                    var key = originalKey.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");
                    var value = config.ShipLogDictionary[originalKey].Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");

                    if (!_shipLogTranslationDictionary[language].ContainsKey(key)) _shipLogTranslationDictionary[language].Add(key, value);
                    else _shipLogTranslationDictionary[language][key] = value;
                }
            }

            if (config.DialogueDictionary != null && config.DialogueDictionary.Count > 0)
            {
                if (!_dialogueTranslationDictionary.ContainsKey(language)) _dialogueTranslationDictionary.Add(language, new Dictionary<string, string>());
                foreach (var originalKey in config.DialogueDictionary.Keys)
                {
                    var key = originalKey.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");
                    var value = config.DialogueDictionary[originalKey].Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");

                    if (!_dialogueTranslationDictionary[language].ContainsKey(key)) _dialogueTranslationDictionary[language].Add(key, value);
                    else _dialogueTranslationDictionary[language][key] = value;
                }
            }

            if (config.UIDictionary != null && config.UIDictionary.Count() > 0)
            {
                if (!_uiTranslationDictionary.ContainsKey(language)) _uiTranslationDictionary.Add(language, new Dictionary<string, string>());
                foreach (var originalKey in config.UIDictionary.Keys)
                {
                    var key = originalKey.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");
                    var value = config.UIDictionary[originalKey].Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");

                    if (!_uiTranslationDictionary[language].ContainsKey(key)) _uiTranslationDictionary[language].Add(key, value);
                    else _uiTranslationDictionary[language][key] = value;
                }
            }

            if (config.OtherDictionary != null && config.OtherDictionary.Count() > 0)
            {
                if (!_otherTranslationDictionary.ContainsKey(language)) _otherTranslationDictionary.Add(language, new Dictionary<string, string>());
                foreach (var originalKey in config.OtherDictionary.Keys)
                {
                    // Don't remove CDATA
                    var key = originalKey.Replace("&lt;", "<").Replace("&gt;", ">");
                    var value = config.OtherDictionary[originalKey].Replace("&lt;", "<").Replace("&gt;", ">");

                    if (!_otherTranslationDictionary[language].ContainsKey(key)) _otherTranslationDictionary[language].Add(key, value);
                    else _otherTranslationDictionary[language][key] = value;
                }
            }
        }

        public static (string, string) FixKeyValue(string key, string value)
        {
            key = key.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");
            value = value.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");

            return (key, value);
        }

        public static void AddDialogue(string rawText, bool trimRawTextForKey = false, params string[] rawPreText)
        {
            var key = string.Join(string.Empty, rawPreText) + (trimRawTextForKey? rawText.Trim() : rawText);

            var value = GetTranslation(rawText, TextType.DIALOGUE);

            // Manually insert directly into the dictionary, otherwise it logs errors about duplicates but we want to allow replacing
            (key, value) = FixKeyValue(key, value);

            TextTranslation.Get().m_table.theTable[key] = value;
        }

        public static void AddShipLog(string rawText, params string[] rawPreText)
        {
            var key = string.Join(string.Empty, rawPreText) + rawText;

            string value = GetTranslation(rawText, TextType.SHIPLOG);

            // Manually insert directly into the dictionary, otherwise it logs errors about duplicates but we want to allow replacing
            (key, value) = FixKeyValue(key, value);

            TextTranslation.Get().m_table.theShipLogTable[key] = value;
        }

        public static int AddUI(string rawText)
        {
            var uiTable = TextTranslation.Get().m_table.theUITable;

            var text = GetTranslation(rawText, TextType.UI).ToUpper();

            var key = uiTable.Keys.Max() + 1;
            try
            {
                // Ensure it doesn't already contain our UI entry
                KeyValuePair<int, string> pair = uiTable.First(x => x.Value.Equals(text));
                if (pair.Equals(default(KeyValuePair<int, string>))) key = pair.Key;
            }
            catch (Exception) { }

            TextTranslation.Get().m_table.theUITable[key] = text;

            return key;
        }

        public static void ClearTables()
        {
            _shipLogTranslationDictionary.Clear();
            _dialogueTranslationDictionary.Clear();
            _uiTranslationDictionary.Clear();
        }
    }
}
