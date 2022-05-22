using System.Runtime.Serialization;
using NewHorizons.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class SingularityModule : VariableSizeModule
    {
        public enum SingularityType
        {
            [EnumMember(Value = @"BlackHole")]
            BlackHole = 0,
            
            [EnumMember(Value = @"WhiteHole")]
            WhiteHole = 1
        }
        
        /// <summary>
        /// Radius of the singularity. Note that this isn't the same as the event horizon, but includes the entire volume that has warped effects in it.
        /// </summary>
        public float Size;
        
        /// <summary>
        /// The name of the white hole or black hole that is paired to this one. If you don't set a value, entering will kill the player
        /// </summary>
        public string PairedSingularity;
        
        /// <summary>
        /// If you want a black hole to load a new star system scene, put its name here.
        /// </summary>
        public string TargetStarSystem;
        
        /// <summary>
        /// Type of singularity (white hole or black hole)
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SingularityType Type;
        
        /// <summary>
        /// Position of the singularity
        /// </summary>
        public MVector3 Position;
        
        /// <summary>
        /// Only for White Holes. Should this white hole repel the player from it.
        /// </summary>
        [DefaultValue(true)]
        public bool MakeZeroGVolume = true;
    }
}
