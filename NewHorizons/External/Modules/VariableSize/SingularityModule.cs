using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class SingularityModule : GeneralPropInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SingularityType
        {
            [EnumMember(Value = @"blackHole")] BlackHole = 0,

            [EnumMember(Value = @"whiteHole")] WhiteHole = 1
        }

        /// <summary>
        /// Scale this object over time
        /// </summary>
        public TimeValuePair[] curve;

        /// <summary>
        /// The uniqueID of the white hole or black hole that is paired to this one. If you don't set a value, entering will kill
        /// the player
        /// </summary>
        public string pairedSingularity;

        /// <summary>
        /// The uniqueID of this white hole or black hole. If not set it will default to the name of the planet
        /// </summary>
        public string uniqueID;

        /// <summary>
        /// Radius of the singularity. Note that this isn't the same as the event horizon, but includes the entire volume that
        /// has warped effects in it.
        /// </summary>
        [Obsolete("size is deprecated, please use horizonRadius and distortRadius instead")] [Range(0f, double.MaxValue)] public float size;

        /// <summary>
        /// Radius of the event horizon (solid part)
        /// </summary>
        [Range(0f, double.MaxValue)] public float horizonRadius;

        /// <summary>
        /// Radius of the distortion effects. Defaults to 2.5 * horizonRadius
        /// </summary>
        [Range(0f, double.MaxValue)] public float distortRadius;

        /// <summary>
        /// If you want a black hole to load a new star system scene, put its name here.
        /// </summary>
        public string targetStarSystem;

        /// <summary>
        /// Type of singularity (white hole or black hole)
        /// </summary>
        public SingularityType type;

        /// <summary>
        /// Whether a black hole emits blue particles upon warping. It doesn't scale, so disabling this for small black holes is recommended
        /// </summary>
        public bool hasWarpEffects = true;

        /// <summary>
        /// Optional override for the render queue. If the singularity is rendering oddly, increasing this to 3000 can help
        /// </summary>
        [Range(2501f, 3500f)] public int renderQueueOverride = 2985;
    }
}