using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.Remote
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
        public RemoteWhiteboardInfo whiteboard;

        /// <summary>
        /// Camera platform that the stones can project to and from
        /// </summary>
        public RemotePlatformInfo platform;

        /// <summary>
        /// Projection stones
        /// </summary>
        public ProjectionStoneInfo[] stones;
    }
}
