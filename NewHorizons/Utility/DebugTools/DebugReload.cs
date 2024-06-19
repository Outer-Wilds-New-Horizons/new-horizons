using NewHorizons.Handlers;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Common.Menus;
using OWML.Utils;
using System;

namespace NewHorizons.Utility.DebugTools
{
    public static class DebugReload
    {

        private static SubmitAction _reloadButton;

        public static void InitializePauseMenu(IPauseMenuManager pauseMenu)
        {
            _reloadButton = pauseMenu.MakeSimpleButton(TranslationHandler.GetTranslation("Reload Configs", TranslationHandler.TextType.UI).ToUpperFixed(), 3, true);
            _reloadButton.OnSubmitAction += ReloadConfigs;
            UpdateReloadButton();
        }

        public static void UpdateReloadButton()
        {
            _reloadButton?.SetButtonVisible(Main.Debug);
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

            Main.Instance.ForceClearCaches = true;


            SearchUtilities.Find("/PauseMenu/PauseMenuManagers").GetComponent<PauseMenuManager>().OnSkipToNextTimeLoop();

            if (Main.Instance.CurrentStarSystem == "EyeOfTheUniverse")
            {
                PlayerData._currentGameSave.warpedToTheEye = true;
                Main.Instance.IsWarpingBackToEye = true;
                EyeDetailCacher.IsInitialized = false;
                Main.Instance.ChangeCurrentStarSystem("SolarSystem");
            }
            else
            {
                Main.Instance.ChangeCurrentStarSystem(Main.Instance.CurrentStarSystem, Main.Instance.DidWarpFromShip, Main.Instance.DidWarpFromVessel);
            }

            Main.SecondsElapsedInLoop = -1f;
        }
    }
}