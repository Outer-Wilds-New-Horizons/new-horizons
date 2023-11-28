using NewHorizons.Handlers;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Common.Menus;
using System;

namespace NewHorizons.Utility.DebugTools
{
    public static class DebugReload
    {

        private static IModButton _reloadButton;

        public static void InitializePauseMenu()
        {
            _reloadButton = Main.Instance.ModHelper.Menus.PauseMenu.OptionsButton.Duplicate(TranslationHandler.GetTranslation("Reload Configs", TranslationHandler.TextType.UI).ToUpper());
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
            NHLogger.Log("Begin reload of config files...");

            Main.Instance.ResetConfigs();

            try
            {
                foreach (IModBehaviour mountedAddon in Main.MountedAddons)
                {
                    Main.Instance.LoadConfigs(mountedAddon);
                }
            }
            catch (Exception)
            {
                NHLogger.LogWarning("Error While Reloading");
            }

            SearchUtilities.Find("/PauseMenu/PauseMenuManagers").GetComponent<PauseMenuManager>().OnSkipToNextTimeLoop();

            Main.Instance.ForceClearCaches = true;
            Main.Instance.ChangeCurrentStarSystem(Main.Instance.CurrentStarSystem);

            Main.SecondsElapsedInLoop = -1f;
        }
    }
}