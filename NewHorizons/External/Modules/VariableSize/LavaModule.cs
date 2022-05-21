using NewHorizons.Utility;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class LavaModule : VariableSizeModule
    {
        /// <summary>
        /// Size of the lava sphere
        /// </summary>
        public float Size;

        /// <summary>
        /// Tint of the lava
        /// </summary>
        public MColor Tint;
    }
}
