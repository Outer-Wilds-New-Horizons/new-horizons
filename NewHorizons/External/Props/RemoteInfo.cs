using System.ComponentModel;
using Newtonsoft.Json;

namespace NewHorizons.External.Props
{
    [JsonObject]
    public class RemoteInfo
    {
        /// <summary>
        /// The unique remote id
        /// </summary>
        public string id;

        /// <summary>
        /// Icon that the will show on the stone, pedastal of the whiteboard, and pedastal of the platform.
        /// </summary>
        public string decalPath;

        /// <summary>
        /// Whiteboard that the stones can put text onto
        /// </summary>
        public WhiteboardInfo whiteboard;

        /// <summary>
        /// Camera platform that the stones can project to and from
        /// </summary>
        public PlatformInfo platform;

        /// <summary>
        /// Projection stones
        /// </summary>
        public StoneInfo[] stones;

        [JsonObject]
        public class WhiteboardInfo : GeneralPropInfo
        {
            /// <summary>
            /// The text for each stone
            /// </summary>
            public SharedNomaiTextInfo[] nomaiText;

            /// <summary>
            /// Disable the wall, leaving only the pedestal and text.
            /// </summary>
            public bool disableWall;

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
                /// The location of this object. 
                /// </summary>
                [DefaultValue("unspecified")] public NomaiTextInfo.NomaiTextLocation location = NomaiTextInfo.NomaiTextLocation.UNSPECIFIED;

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

        [JsonObject]
        public class PlatformInfo : GeneralPropInfo
        {
            /// <summary>
            /// A ship log fact to reveal when the platform is connected to.
            /// </summary>
            [DefaultValue("")] public string reveals = "";

            /// <summary>
            /// Disable the structure, leaving only the pedestal.
            /// </summary>
            public bool disableStructure;

            /// <summary>
            /// Disable the pool that rises when you place a stone.
            /// </summary>
            public bool disablePool;
        }

        [JsonObject]
        public class StoneInfo : GeneralPropInfo
        {

        }
    }
}
