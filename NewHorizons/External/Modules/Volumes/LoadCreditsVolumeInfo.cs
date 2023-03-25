using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes
{
    [JsonObject]
    public class LoadCreditsVolumeInfo : VolumeInfo
    {
        [DefaultValue("fast")] public CreditsType creditsType = CreditsType.Fast;

        /// <summary>
        /// Text displayed in orange on game over. For localization, put translations under UI.
        /// </summary>
        public string gameOverText;

        /// <summary>
        /// The type of death the player will have if they enter this volume.
        /// </summary>
        [DefaultValue("default")] public DeathType deathType = DeathType.Default;
    }
}
