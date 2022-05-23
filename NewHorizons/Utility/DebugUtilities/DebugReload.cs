using NewHorizons.Handlers;
using OWML.Common.Menus;
using System;
using UnityEngine;

namespace NewHorizons.Utility.DebugUtilities
{
    public static class DebugReload
    {
        private static IModButton _reloadButton;

        public static void InitializePauseMenu()
        {
            _reloadButton = Main.Instance.ModHelper.Menus.PauseMenu.OptionsButton.Duplicate(TranslationHandler
                .GetTranslation("Reload Configs", TranslationHandler.TextType.UI).ToUpper());
            _reloadButton.OnClick += ReloadConfigs;
            UpdateReloadButton();
        }

        public static void UpdateReloadButton()
        {
            if (_reloadButton != null)
            {
                if (Main.Debug) _reloadButton.Show();
                else _reloadButton.Hide();
            }
        }

        private static void ReloadConfigs()
        {
            Logger.Log("Begin reload of config files...", Logger.LogType.Log);

            Main.ResetConfigs();

            try
            {
                foreach (var mountedAddon in Main.MountedAddons) Main.Instance.LoadConfigs(mountedAddon);
            }
            catch (Exception)
            {
                Logger.LogWarning("Error While Reloading");
            }

            GameObject.Find("/PauseMenu/PauseMenuManagers").GetComponent<PauseMenuManager>().OnSkipToNextTimeLoop();

            Main.Instance.ChangeCurrentStarSystem(Main.Instance.CurrentStarSystem);

            Main.SecondsLeftInLoop = -1f;
        }
    }
}