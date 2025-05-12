using NewHorizons.External;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TextTranslation;

namespace NewHorizons.OtherMods.MenuFramework
{
    public static class MenuHandler
    {
        private static List<(IModBehaviour mod, string message, bool repeat)> _registeredPopups = new();
        private static List<string> _failedFiles = new();

        public static void Init()
        {
            TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
        }

        public static void OnLanguageChanged()
        {
            // Have to load save data before doing popups
            NewHorizonsData.Load();

            if (!VersionUtility.CheckUpToDate())
            {
                var warning = string.Format(TranslationHandler.GetTranslation("OUTDATED_VERSION_WARNING", TranslationHandler.TextType.UI),
                    VersionUtility.RequiredVersionString,
                    Application.version);

                NHLogger.LogError(warning);
                Main.Instance.ModHelper.MenuHelper.PopupMenuManager.RegisterStartupPopup(warning);
            }

            foreach(var (mod, message, repeat) in _registeredPopups)
            {
                if (repeat || !NewHorizonsData.HasReadOneTimePopup(mod.ModHelper.Manifest.UniqueName))
                {
                    Main.Instance.ModHelper.MenuHelper.PopupMenuManager.RegisterStartupPopup(TranslationHandler.GetTranslation(message, TranslationHandler.TextType.UI));
                    NewHorizonsData.ReadOneTimePopup(mod.ModHelper.Manifest.UniqueName);
                }
            }

            if (_failedFiles.Count > 0)
            {
                var message = TranslationHandler.GetTranslation("JSON_FAILED_TO_LOAD", TranslationHandler.TextType.UI);
                var mods = string.Join(",", _failedFiles.Take(10));
                if (_failedFiles.Count > 10) mods += "...";
                Main.Instance.ModHelper.MenuHelper.PopupMenuManager.RegisterStartupPopup(string.Format(message, mods));
            }

            _registeredPopups.Clear();
            _failedFiles.Clear();

            // Just wanted to do this when the language is loaded in initially
            TextTranslation.Get().OnLanguageChanged -= OnLanguageChanged;
        }

        public static void RegisterFailedConfig(string filename) => _failedFiles.Add(filename);

        public static void RegisterOneTimePopup(IModBehaviour mod, string message, bool repeat) => _registeredPopups.Add((mod, message, repeat));

        public static void TitleScreen()
        {
            // Custom popup for recommending the Chinese Outer Wilds Font Fix mod if they are playing in chinese
            // Only shows once per profile
            if (TextTranslation.Get().m_language == Language.CHINESE_SIMPLE 
                && !Main.Instance.ModHelper.Interaction.ModExists("nice2cu1.OuterWildFixFont") 
                && !NewHorizonsData.HasReadOneTimePopup("INSTALL_OUTER_WILDS_CHINESE_FONT_FIX"))
            {
                Main.Instance.ModHelper.MenuHelper.PopupMenuManager.RegisterStartupPopup(TranslationHandler.GetTranslation("INSTALL_OUTER_WILDS_CHINESE_FONT_FIX", TranslationHandler.TextType.UI));
                NewHorizonsData.ReadOneTimePopup("INSTALL_OUTER_WILDS_CHINESE_FONT_FIX");
            }
        }
    }
}
