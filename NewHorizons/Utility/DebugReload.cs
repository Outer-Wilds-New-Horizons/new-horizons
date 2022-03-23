using System;
using System.Collections.Generic;
using NewHorizons.External.Configs;
using NewHorizons.Handlers;
using OWML.Common;
using OWML.Common.Menus;
using UnityEngine;

namespace NewHorizons.Utility
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
            Main.BodyDict.Clear();
            Main.SystemDict.Clear();

            Main.BodyDict["SolarSystem"] = new List<NewHorizonsBody>();
            Main.SystemDict["SolarSystem"] = new NewHorizonsSystem("SolarSystem", new StarSystemConfig(null), Main.Instance);
            foreach (AssetBundle bundle in Main.AssetBundles.Values)
            {
                bundle.Unload(true);
            }
            Main.AssetBundles.Clear();
            
            Logger.Log("Begin reload of config files...", Logger.LogType.Log);

            try
            {
                foreach (IModBehaviour mountedAddon in Main.MountedAddons)
                {
                    Main.Instance.LoadConfigs(mountedAddon);
                }
            }
            catch (Exception)
            {
                Logger.LogWarning("Error While Reloading");
            }
            
            GameObject.Find("/PauseMenu/PauseMenuManagers").GetComponent<PauseMenuManager>().OnSkipToNextTimeLoop();
            
            Main.Instance.ChangeCurrentStarSystem(Main.Instance.CurrentStarSystem);
        }
    }
}