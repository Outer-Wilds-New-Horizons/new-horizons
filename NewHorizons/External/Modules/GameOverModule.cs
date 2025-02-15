using NewHorizons.External.SerializableData;
using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class GameOverModule
    {
        /// <summary>
        /// Text displayed in orange on game over. For localization, put translations under UI.
        /// </summary>
        public string text;

        /// <summary>
        /// Change the colour of the game over text. Leave empty to use the default orange.
        /// </summary>
        public MColor colour;

        /// <summary>
        /// Condition that must be true for this game over to trigger. If this is on a LoadCreditsVolume, leave empty to always trigger this game over.
        /// Note this is a regular dialogue condition, not a persistent condition.
        /// </summary>
        public string condition;

        /// <summary>
        /// The type of credits that will run after the game over message is shown
        /// </summary>
        [DefaultValue("fast")] public NHCreditsType creditsType = NHCreditsType.Fast;
    }
}
