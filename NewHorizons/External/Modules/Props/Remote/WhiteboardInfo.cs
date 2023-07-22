using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.Remote
{
    [JsonObject]
    public class RemoteWhiteboardInfo : GeneralPropInfo
    {
        /// <summary>
        /// The text for each stone
        /// </summary>
        public SharedNomaiTextInfo[] nomaiText;

        /// <summary>
        /// Disable the wall, leaving only the pedestal and text.
        /// </summary>
        public bool disableWall;
    }
}
