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

        /// <summary>
        /// Path to the dock this raft will start attached to.
        /// </summary>
        public string dockPath;

        /// <summary>
        /// Uses the raft model from the dreamworld
        /// </summary>
        public bool clean;
    }

}
