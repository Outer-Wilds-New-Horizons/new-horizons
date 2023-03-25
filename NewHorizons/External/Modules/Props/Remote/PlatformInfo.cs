using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Remote
{
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
}
