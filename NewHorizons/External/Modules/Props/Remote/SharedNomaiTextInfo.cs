using NewHorizons.External.Modules.TranslatorText;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Remote
{
    [JsonObject]
    public class SharedNomaiTextInfo
    {
        /// <summary>
        /// The id of the stone this text will appear for
        /// </summary>
        public string id;

        /// <summary>
        /// Additional information about each arc in the text
        /// </summary>
        public NomaiTextArcInfo[] arcInfo;

        /// <summary>
        /// The random seed used to pick what the text arcs will look like.
        /// </summary>
        public int seed; // For randomizing arcs

        /// <summary>
        /// The location of this object. Arcs will be blue if their locations match the wall, else orange.
        /// </summary>
        [DefaultValue("unspecified")] public NomaiTextLocation location = NomaiTextLocation.UNSPECIFIED;

        /// <summary>
        /// The relative path to the xml file for this object.
        /// </summary>
        public string xmlFile;

        /// <summary>
        /// An optional rename of this object
        /// </summary>
        public string rename;
    }
}
