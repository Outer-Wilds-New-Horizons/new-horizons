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
        /// The audio to use for the credits music. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// Credits will be silent unless this attribute is specified.
        /// Note: only applies when creditsType is set to "custom".
        /// </summary>
        public string audio;

        /// <summary>
        /// The length of the fade in and out for the credits music.
        /// Note: only applies when creditsType is set to "custom".
        /// </summary>
        [DefaultValue(1f)] public float audioVolume = 1f;

        /// <summary>
        /// Determines if the credits music should loop.
        /// Note: only applies when creditsType is set to "custom".
        /// </summary>
        [DefaultValue(false)] public bool audioLooping = false;

        /// <summary>
        /// Duration of the credits scroll in seconds.
        /// Note: only applies when creditsType is set to "custom".
        /// </summary>
        [DefaultValue(120f)] public float length = 120f;

        /// <summary>
        /// The type of credits that will run after the game over message is shown
        /// </summary>
        [DefaultValue("fast")] public NHCreditsType creditsType = NHCreditsType.Fast;
    }
}
