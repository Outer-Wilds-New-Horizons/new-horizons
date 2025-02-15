using NewHorizons.External.SerializableData;
using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class LoadCreditsVolumeInfo : VolumeInfo
    {
        [DefaultValue("none")] public NHCreditsType creditsType = NHCreditsType.None;

        /// <summary>
        /// Text displayed in orange on game over. For localization, put translations under UI.
        /// </summary>
        public string gameOverText;

        /// <summary>
        /// The type of death the player will have if they enter this volume. Don't set to have the camera just fade out.
        /// </summary>
        [DefaultValue("default")] public NHDeathType? deathType = null;

        /// <summary>
        /// Change the colour of the game over text. Leave empty to use the default orange.
        /// </summary>
        public MColor gameOverTextColour;
    }
}
