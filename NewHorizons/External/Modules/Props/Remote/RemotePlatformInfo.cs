using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Remote
{
    [JsonObject]
    public class RemotePlatformInfo : GeneralPropInfo
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

        /// <summary>
        /// Trigger volumes to temporarily add the player to when viewing the remote projection. The player will be removed from the volumes when the projection stops.
        /// </summary>
        public string[] entrywayVolumes;
    }
}
