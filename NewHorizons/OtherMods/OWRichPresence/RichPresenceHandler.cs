using NewHorizons.Components.ShipLog;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.OtherMods.OWRichPresence
{
    public class RichPresenceHandler
    {
        public static bool Enabled { get; private set; }

        private static IRichPresenceAPI API;

        public static void Init()
        {
            try
            {
                API = Main.Instance.ModHelper.Interaction.TryGetModApi<IRichPresenceAPI>("MegaPiggy.OWRichPresence");

                if (API == null)
                {
                    Logger.LogVerbose("OWRichPresence isn't installed");
                    Enabled = false;
                    return;
                }

                Enabled = true;
            }
            catch(Exception ex)
            {
                Logger.LogError($"OWRichPresence handler failed to initialize: {ex}");
                Enabled = false;
            }
        }

        public static void SetUpPlanet(string name, GameObject go, Sector sector, bool isStar = false, bool hasAtmosphere = false)
        {
            if (!Enabled) return;

            Logger.LogVerbose($"Registering {go.name} to OWRichPresence");

            var localizedName = TranslationHandler.GetTranslation(name, TranslationHandler.TextType.UI);
            var message = TranslationHandler.GetTranslation("RICH_PRESENCE_EXPLORING", TranslationHandler.TextType.UI).Replace("{0}", localizedName);

            string fallbackKey = "defaultplanet";
            if (isStar) fallbackKey = "defaultstar";
            else if (hasAtmosphere) fallbackKey = "defaultplanetatmosphere";

            API.CreateTrigger(go, sector, message, name.Replace(" ", "").Replace("'", "").Replace("-", "").ToLowerInvariant(), fallbackKey);
        }

        public static void OnStarSystemLoaded(string name)
        {
            if (!Enabled) return;

            if (name == "SolarSystem") return;

            var localizedName = ShipLogStarChartMode.UniqueIDToName(name);
            var message = TranslationHandler.GetTranslation("RICH_PRESENCE_EXPLORING", TranslationHandler.TextType.UI).Replace("{0}", localizedName);

            API.SetCurrentRootPresence(message, "newhorizons");
        }

        public static void OnChangeStarSystem(string destination)
        {
            if (!Enabled) return;

            var localizedName = ShipLogStarChartMode.UniqueIDToName(destination);
            var message = TranslationHandler.GetTranslation("RICH_PRESENCE_WARPING", TranslationHandler.TextType.UI).Replace("{0}", localizedName);

            API.SetRichPresence(message, "newhorizons");
        }
    }
}
