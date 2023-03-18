
using NewHorizons.Utility;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Props
{
    [JsonObject]
    public class NomaiTextArcInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum NomaiTextArcType
        {
            [EnumMember(Value = @"adult")] Adult = 0,

            [EnumMember(Value = @"child")] Child = 1,

            [EnumMember(Value = @"stranger")] Stranger = 2
        }

        /// <summary>
        /// Whether to skip modifying this spiral's placement, and instead keep the automatically determined placement.
        /// </summary>
        public bool keepAutoPlacement;

        /// <summary>
        /// Whether to flip the spiral from left-curling to right-curling or vice versa.
        /// </summary>
        public bool mirror;

        /// <summary>
        /// The local position of this object on the wall.
        /// </summary>
        public MVector2 position;

        /// <summary>
        /// The type of text to display.
        /// </summary>
        [DefaultValue("adult")] public NomaiTextArcType type = NomaiTextArcType.Adult;

        /// <summary>
        /// Which variation of the chosen type to place. If not specified, a random variation will be selected based on the seed provided in the parent module.
        /// </summary>
        [DefaultValue(-1)] public int variation = -1;

        /// <summary>
        /// The z euler angle for this arc.
        /// </summary>
        [Range(0f, 360f)] public float zRotation;
    }
}
