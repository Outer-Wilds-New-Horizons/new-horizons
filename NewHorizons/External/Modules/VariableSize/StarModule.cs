using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NewHorizons.Utility;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class StarModule : VariableSizeModule
    {
        /// <summary>
        /// Colour of the star at the end of its lifespan.
        /// </summary>
        public MColor endTint;

        /// <summary>
        /// Should this star explode at the end of its lifespan?
        /// </summary>
        [DefaultValue(true)] public bool goSupernova = true;

        /// <summary>
        /// How long in minutes this star will last until it supernovas.
        /// </summary>
        [DefaultValue(22f)]
        public float lifespan = 22f;

        /// <summary>
        /// Should we add a star controller to this body? If you want clouds to work on a binary brown dwarf system, set this to false.
        /// </summary>
        [DefaultValue(true)] public bool hasStarController = true;

        /// <summary>
        /// The default sun has its own atmosphere that is different from regular planets. If you want that, set this to
        /// `true`.
        /// </summary>
        [DefaultValue(true)] public bool hasAtmosphere = true;

        /// <summary>
        /// Colour of the light given off. Defaults to yellowish.
        /// </summary>
        public MColor lightTint;

        /// <summary>
        /// Radius of the star.
        /// </summary>
        [DefaultValue(2000f)] [Range(0f, double.MaxValue)]
        public float size = 2000f;

        /// <summary>
        /// Relative strength of the light compared to the sun.
        /// </summary>
        [DefaultValue(1f)] [Range(0f, double.MaxValue)]
        public float solarLuminosity = 1f;

        /// <summary>
        /// Radius of the supernova. Any planets within 80% of this will be destroyed.
        /// </summary>
        [DefaultValue(50000f)]
        [Range(0f, double.MaxValue)]
        public float supernovaSize = 50000f;

        /// <summary>
        /// The tint of the supernova this star creates when it dies.
        /// </summary>
        public MColor supernovaTint;

        /// <summary>
        /// Colour of the star.
        /// </summary>
        public MColor tint;

        /// <summary>
        /// Path to the texture to put as the star ramp. Optional.
        /// </summary>
        public string starRampTexture;

        /// <summary>
        /// Path to the texture to put as the star ramp while it is collapsing. Optional.
        /// </summary>
        public string starCollapseRampTexture;

        /// <summary>
        /// How far the light from the star can reach.
        /// </summary>
        [DefaultValue(50000f)] [Range(0f, double.MaxValue)]
        public float lightRadius = 50000f;
    }
}
