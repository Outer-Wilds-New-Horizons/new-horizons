using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.TranslatorText
{

    [JsonObject]
    public class NomaiTextInfo : GeneralPointPropInfo
    {

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
        /// The location of this object. Arcs will be blue if their locations match the wall, else orange.
        /// </summary>
        [DefaultValue("unspecified")] public NomaiTextLocation location = NomaiTextLocation.UNSPECIFIED;

        /// <summary>
        /// The relative path to the xml file for this object.
        /// </summary>
        public string xmlFile;
    }

}
