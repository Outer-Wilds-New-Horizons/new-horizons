using NewHorizons.Components;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System.IO;
using System.Linq;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.OtherMods.OWRichPresence
{
    public class RichPresenceHandler
    {
        public static bool Enabled { get; private set; }

        private static IRichPresenceAPI API;

        public static void Init()
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

        public static void SetUpPlanet(string name, GameObject go, Sector sector)
        {
            if (!Enabled) return;

            Logger.LogVerbose($"Registering {go.name} to OWRichPresence");

            var localizedName = TranslationHandler.GetTranslation(name, TranslationHandler.TextType.UI);
            var localizedMessage = TranslationHandler.GetTranslation("OWRP_EXPLORING", TranslationHandler.TextType.UI);
            var message = localizedMessage.Replace("{0}", localizedName);

            API.CreateTrigger(go, sector, message, "sun");
        }

        public static void SetUpSolarSystem(string name)
        {
            if (name == "SolarSystem") return;

            var localizedName = ShipLogStarChartMode.UniqueIDToName(name);
            var localizedMessage = TranslationHandler.GetTranslation("OWRP_EXPLORING", TranslationHandler.TextType.UI);
            var message = localizedMessage.Replace("{0}", localizedName);

            API.SetCurrentRootPresence(message, "sun");
        }
    }
}
