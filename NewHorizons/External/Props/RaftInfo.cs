using System.ComponentModel;
using Newtonsoft.Json;

namespace NewHorizons.External.Props
{
    [JsonObject]
    public class RaftInfo : GeneralPointPropInfo
    {
        /// <summary>
        /// Acceleration of the raft. Default acceleration is 5.
        /// </summary>
        [DefaultValue(5f)] public float acceleration = 5f;
    }
}
