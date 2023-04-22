using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.TranslatorText
{

    [JsonObject]
    public class NomaiTextArcInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum NomaiTextArcType
        {
            [EnumMember(Value = @"adult")] Adult = 0,

            [EnumMember(Value = @"child")] Child = 1,

            [EnumMember(Value = @"stranger")] Stranger = 2,

            [EnumMember(Value = @"teenager")] Teenager = 3
        }

        /// <summary>
        /// The type of text to display.
        /// </summary>
        [DefaultValue("adult")] public NomaiTextArcType type = NomaiTextArcType.Adult;

        /// <summary>
        /// The local position of this object on the wall. If specified, auto spiral will not touch this arc.
        /// </summary>
        public MVector2 position;

        /// <summary>
        /// The z euler angle for this arc. If specified, auto spiral will not touch this arc.
        /// </summary>
        [Range(0f, 360f)] public float? zRotation;

        /// <summary>
        /// Whether to flip the spiral from left-curling to right-curling or vice versa. If specified, auto spiral will not touch this arc.
        /// </summary>
        public bool? mirror;

        /// <summary>
        /// Which variation of the chosen type to place. If not specified, a random variation will be selected based on the seed provided in the parent module.
        /// </summary>
        [Obsolete("only used in old nomai text")]
        [DefaultValue(-1)] public int variation = -1;
    }

}
