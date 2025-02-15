using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.SerializableData;
using NewHorizons.Handlers;
using Newtonsoft.Json;

namespace NewHorizons.External.Configs
{
    [JsonObject]
    public class TitleScreenConfig
    {
        /// <summary>
        /// Colour of the text on the main menu
        /// </summary>
        public MColor menuTextTint;

        /// <summary>
        /// Ship log fact required for this title screen to appear.
        /// </summary>
        public string factRequiredForTitle;

        /// <summary>
        /// Persistent condition required for this title screen to appear.
        /// </summary>
        public string conditionRequiredForTitle;

        /// <summary>
        /// If set to true, NH generated planets will not show on the title screen. If false, this title screen has the same chance as other NH planet title screens to show.
        /// </summary>
        public bool disableNHPlanets = true;

        /// <summary>
        /// If set to true, this custom title screen will merge with all other custom title screens with shareTitleScreen set to true. If false, NH will randomly select between this and other valid title screens that are loaded.
        /// </summary>
        public bool shareTitleScreen = true;

        /// <summary>
        /// Customize the skybox for this title screen
        /// </summary>
        public SkyboxModule Skybox;

        /// <summary>
        /// The music audio that will play on the title screen. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string music;

        /// <summary>
        /// The ambience audio that will play on the title screen. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string ambience;

        /// <summary>
        /// Edit properties of the background
        /// </summary>
        public BackgroundModule Background;

        /// <summary>
        /// Edit properties of the main menu planet
        /// </summary>
        public MenuPlanetModule MenuPlanet;

        [JsonObject]
        public class BackgroundModule
        {
            /// <summary>
            /// Changes the speed the background rotates (and by extension the main menu planet). This is in degrees per second.
            /// </summary>
            public float rotationSpeed = 1;

            /// <summary>
            /// Disables the renderers of objects at the provided paths
            /// </summary>
            public string[] removeChildren;

            /// <summary>
            /// A list of DetailInfos to populate the background with.
            /// </summary>
            public SimplifiedDetailInfo[] details;
        }

        [JsonObject]
        public class MenuPlanetModule
        {
            /// <summary>
            /// Disables the renderers of the main menu planet and all objects on it (this is to improve compatibility with other mods that don't use the NH title screen json).
            /// </summary>
            public bool destroyMenuPlanet = false;

            /// <summary>
            /// Disables the renderers of objects at the provided paths
            /// </summary>
            public string[] removeChildren;

            /// <summary>
            /// A list of DetailInfos to populate the main menu planet with.
            /// </summary>
            public SimplifiedDetailInfo[] details;

            /// <summary>
            /// Changes the speed the main menu planet. This is in degrees per second.
            /// </summary>
            public float rotationSpeed = 2;
        }

        /// <summary>
        /// Extra data that may be used by extension mods
        /// </summary>
        public object extras;
    }
}
