using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props
{
    [JsonObject]
    public class EntryLocationInfo : GeneralPointPropInfo
    {
        /// <summary>
        /// Whether this location is cloaked
        /// </summary>
        public bool cloaked;

        /// <summary>
        /// ID of the entry this location relates to
        /// </summary>
        public string id;
    }
}
