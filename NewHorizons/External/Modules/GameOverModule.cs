using NewHorizons.External.SerializableData;
using Newtonsoft.Json;

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
        /// Condition that must be true for this game over to trigger. Leave empty to always trigger this game over.
        /// Note this is a regular dialogue condition, not a persistent condition.
        /// </summary>
        public string condition;
    }
}
