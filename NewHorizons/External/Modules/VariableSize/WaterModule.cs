using NewHorizons.Utility;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class WaterModule : VariableSizeModule
    {
        /// <summary>
        /// Size of the water sphere
        /// </summary>
        public float Size;

        /// <summary>
        /// Tint of the water
        /// </summary>
        public MColor Tint;
    }
}
