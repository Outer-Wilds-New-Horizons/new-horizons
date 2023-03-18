
using NewHorizons.Utility;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Props
{
    [JsonObject]
    public class NomaiTextInfo : GeneralPointPropInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum NomaiTextType
        {
            [EnumMember(Value = @"wall")] Wall = 0,

            [EnumMember(Value = @"scroll")] Scroll = 1,

            [EnumMember(Value = @"computer")] Computer = 2,

            [EnumMember(Value = @"cairn")] Cairn = 3,

            [EnumMember(Value = @"recorder")] Recorder = 4,

            [EnumMember(Value = @"preCrashRecorder")] PreCrashRecorder = 5,

            [EnumMember(Value = @"preCrashComputer")] PreCrashComputer = 6,

            [EnumMember(Value = @"trailmarker")] Trailmarker = 7,

            [EnumMember(Value = @"cairnVariant")] CairnVariant = 8,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum NomaiTextLocation
        {
            [EnumMember(Value = @"unspecified")] UNSPECIFIED = 0,

            [EnumMember(Value = @"a")] A = 1,

            [EnumMember(Value = @"b")] B = 2
        }

        /// <summary>
        /// Additional information about each arc in the text
        /// </summary>
        public NomaiTextArcInfo[] arcInfo;

        /// <summary>
        /// The normal vector for this object. Used for writing on walls and positioning computers.
        /// </summary>
        public MVector3 normal;

        /// <summary>
        /// The euler angle rotation of this object. Not required if setting the normal. Computers and cairns will orient
        /// themselves to the surface of the planet automatically.
        /// </summary>
        public MVector3 rotation;

        /// <summary>
        /// The random seed used to pick what the text arcs will look like.
        /// </summary>
        public int seed; // For randomizing arcs

        /// <summary>
        /// The type of object this is.
        /// </summary>
        [DefaultValue("wall")] public NomaiTextType type = NomaiTextType.Wall;

        /// <summary>
        /// The location of this object. 
        /// </summary>
        [DefaultValue("unspecified")] public NomaiTextLocation location = NomaiTextLocation.UNSPECIFIED;

        /// <summary>
        /// The relative path to the xml file for this object.
        /// </summary>
        public string xmlFile;
    }
}
