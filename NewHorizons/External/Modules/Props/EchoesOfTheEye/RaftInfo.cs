using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    [JsonObject]
    public class RaftInfo : GeneralPropInfo
    {
        /// <summary>
        /// Acceleration of the raft. Default acceleration is 5.
        /// </summary>
        [DefaultValue(5f)] public float acceleration = 5f;
    }

}
