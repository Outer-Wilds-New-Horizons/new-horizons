using NewHorizons.OtherMods.AchievementsPlus;
using Newtonsoft.Json;

namespace NewHorizons.External.Configs
{
    /// <summary>
    /// Describes the New Horizons addon itself
    /// </summary>
    [JsonObject]
    public class AddonConfig
    {
        /// <summary>
        /// Achievements for this mod if the user is playing with Achievements+
        /// Achievement icons must be put in a folder named "icons"
        /// The icon for the mod must match the name of the mod (e.g., New Horizons.png)
        /// The icons for achievements must match their unique IDs (e.g., NH_WARP_DRIVE.png)
        /// </summary>
        public AchievementInfo[] achievements;

        /// <summary>
        /// Credits info for this mod. A list of contributors and their roles separated by #. For example: xen#New Horizons dev.
        /// </summary>
        public string[] credits;

        /// <summary>
        /// A pop-up message for the first time a user runs the add-on
        /// </summary>
        public string popupMessage;

        /// <summary>
        /// If popupMessage is set, should it repeat every time the game starts or only once
        /// </summary>
        public bool repeatPopup;

        /// <summary>
        /// These asset bundles will be loaded on the title screen and stay loaded. Will improve initial load time at the cost of increased memory use.
        /// The path is the relative directory of the asset bundle in the mod folder.
        /// </summary>
        public string[] preloadAssetBundles;

        /// <summary>
        /// The path to the addons subtitle for the main menu.
        /// Defaults to "subtitle.png".
        /// The dimensions of the Echos of the Eye subtitle is 669 x 67, so aim for that size
        /// </summary>
        public string subtitlePath = "subtitle.png";
    }
}
