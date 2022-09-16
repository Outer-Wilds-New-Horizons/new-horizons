using NewHorizons.External.Configs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class TranslationHandler
    {
        // Dictionary loaded from mods directly
        private static readonly Dictionary<TextTranslation.Language, Dictionary<string, string>> _shipLogTranslationDictionary = new();
        private static readonly Dictionary<TextTranslation.Language, Dictionary<string, string>> _dialogueTranslationDictionary = new();
        private static readonly Dictionary<TextTranslation.Language, Dictionary<string, string>> _uiTranslationDictionary = new();

        // Values we append to the base game TextTranslation dictionary for the currently selected language
        private static readonly List<(string key, string rawText)> _shipLogTable = new(); 
        private static readonly List<(string key, string rawText)> _dialogueTable = new(); 
        private static readonly List<(int key, string text)> _uiTable = new(); 
        public enum TextType
        {
            SHIPLOG,
            DIALOGUE,
            UI
        }
        public static void OnLanguageChanged()
        {
            if (SceneManager.GetActiveScene().name != "TitleScreen")
            {
                Logger.LogError("Language was changed outside of main menu. Please tell an NH dev about this!");
            }

            // Re-add everything to the list
            OnSceneFinishLoading();
        }

        public static void OnSceneFinishLoading()
        {
            var table = TextTranslation.Get().m_table;

            foreach (var (key, rawText) in _dialogueTable)
                if (!table.theTable.ContainsKey(key))
                    table.Insert(key, GetTranslation(rawText, TextType.DIALOGUE));

            foreach (var (key, rawText) in _shipLogTable)
                if (!table.theShipLogTable.ContainsKey(key))
                    TextTranslation.Get().m_table.InsertShipLog(key, GetTranslation(rawText, TextType.SHIPLOG));

            foreach (var (key, text) in _uiTable)
                if (!table.theUITable.ContainsKey(key))
                    TextTranslation.Get().m_table.Insert_UI(key, text);
        }

        public static void OnSceneUnloaded()
        {
            _dialogueTable.Clear();
            _shipLogTable.Clear();
            _uiTable.Clear();
        }
        public static string GetTranslation(string text, TextType type)
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
                default:
                    Logger.LogVerbose($"Invalid TextType {type}");
                    return text;
            }

            // Get the translated text
            if (dictionary.TryGetValue(language, out var table))
                if (table.TryGetValue(text, out var translatedText))
                    return translatedText;

            Logger.LogVerbose($"Defaulting to english for {text}");

            // Try to default to English
            if (dictionary.TryGetValue(TextTranslation.Language.ENGLISH, out var englishTable))
                if (englishTable.TryGetValue(text, out var englishText))
                    return englishText;

            Logger.LogVerbose($"Defaulting to key for {text}");

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
        }

        public static void AddDialogue(string rawText, bool trimRawTextForKey = false, params string[] rawPreText)
        {
            var key = string.Join(string.Empty, rawPreText) + (trimRawTextForKey? rawText.Trim() : rawText);
            _dialogueTable.Add((key, rawText));
        }

        public static void AddShipLog(string rawText, params string[] rawPreText)
        {
            var key = string.Join(string.Empty, rawPreText) + rawText;
            _shipLogTable.Add((key, rawText));
        }

        public static int AddUI(string rawText)
        {
            var uiTable = TextTranslation.Get().m_table.theUITable;

            var text = GetTranslation(rawText, TextType.UI).ToUpper();

            var key = uiTable.Keys.Max() + _uiTable.Count + 1;
            try
            {
                // Ensure it doesn't already contain our UI entry
                KeyValuePair<int, string> pair = uiTable.First(x => x.Value.Equals(text));
                if (pair.Equals(default(KeyValuePair<int, string>))) key = pair.Key;
            }
            catch (Exception) { }

            _uiTable.Add((key, text));

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
