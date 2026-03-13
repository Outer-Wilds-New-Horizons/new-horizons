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

            [EnumMember(Value = @"teenager")] Teenager = 3,
            
            [EnumMember(Value = @"custom")] Custom = 4,
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

        /// <summary>
        /// Allows you to create custom alien language text by overriding the displayed image. This will automatically set the <see cref="type"/> to <see cref="NomaiTextArcType.Custom"/>.
        /// </summary>
        public string customTextImage = null;

        /// <summary>
        /// Makes this text require a persistent condition to be known before it can be translated. 
        /// If you want it to always be untranslatable, use a condition that you will never set like "UNTRANSLATABLE".
        /// </summary>
        public string legiblePersistentCondition = null;

        /// <summary>
        /// Replaces the "Nomai" part in "Untranslated Nomai writing". Translated in the OtherDictionary.
        /// If <see cref="type"/> is set to <see cref="NomaiTextArcType.Custom"/>, this will default to "unknown".
        /// </summary>
        public string customLanguageName = null;

        /// <summary>
        /// Overrides the default unread color of the text arc.
        /// </summary>
        public MColor overrideUnreadColor;

        /// <summary>
        /// Overrides the default translated color of the text arc. This requires that <see cref="overrideUnreadColor"/> is also set. 
        /// If <see cref="overrideUnreadColor"/> is set but this is not, the translated color will be a desaturated version of the unread color.
        /// </summary>
        public MColor overrideTranslatedColor;
    }

}
