using System.ComponentModel;
using NewHorizons.Utility;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class StarModule : VariableSizeModule
    {
        /// <summary>
        /// Colour of the star at the end of its life.
        /// </summary>
        public MColor endTint;

        /// <summary>
        /// Should this star explode after 22 minutes?
        /// </summary>
        [DefaultValue(true)] public bool goSupernova = true;

        /// <summary>
        /// The default sun has its own atmosphere that is different from regular planets. If you want that, set this to
        /// `true`.
        /// </summary>
        [DefaultValue(true)] public bool hasAtmosphere = true;

        /// <summary>
        /// Colour of the light given off.
        /// </summary>
        public MColor lightTint;

        /// <summary>
        /// Radius of the star.
        /// </summary>
        [DefaultValue(2000f)] public float size = 2000f;

        /// <summary>
        /// Relative strength of the light compared to the sun.
        /// </summary>
        [DefaultValue(1f)] public float solarLuminosity = 1f;

        /// <summary>
        /// The tint of the supernova this star creates when it dies.
        /// </summary>
        public MColor supernovaTint;

        /// <summary>
        /// Colour of the star.
        /// </summary>
        public MColor tint;
    }
}