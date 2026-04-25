using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.TranslatorText
{
    [JsonObject]
    public class TranslatorTextInfo : GeneralPropInfo
    {
        /// <summary>
        /// Additional information about each arc in the text
        /// </summary>
        public NomaiTextArcInfo[] arcInfo;

        /// <summary>
        /// Turns this computer off when this dialogue condition is set, and back on when it is unset, or the other way around if `computerStartsOff` is true.
        /// Only revelent when type is `computer` or `preCrashComputer`.
        /// </summary>
        public string computerCondition;

        /// <summary>
        /// Makes this computer turned off by default so the player cannot read the text.
        /// Only revelent when type is `computer` or `preCrashComputer`.
        /// </summary>
        public bool computerStartsOff;

        /// <summary>
        /// The random seed used to pick what the text arcs will look like.
        /// </summary>
        public int seed;

        /// <summary>
        /// Only for wall text. Aligns wall text to face towards the given direction, with 'up' oriented relative to its current rotation or alignment.
        /// </summary>
        public MVector3 normal;

        /// <summary>
        /// The type of object this is.
        /// </summary>
        [DefaultValue("wall")] public NomaiTextType type = NomaiTextType.Wall;

        /// <summary>
        /// The location of this object. Arcs will be blue if their locations match the wall, else orange.
        /// </summary>
        [DefaultValue("unspecified")] public NomaiTextLocation location = NomaiTextLocation.UNSPECIFIED;

        /// <summary>
        /// The relative path to the xml file for this object.
        /// </summary>
        public string xmlFile;
    }

}
