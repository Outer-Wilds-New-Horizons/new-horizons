#region

using NewHorizons.Utility;
using Newtonsoft.Json;

#endregion

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class WaterModule : VariableSizeModule
    {
        /// <summary>
        /// Size of the water sphere
        /// </summary>
        public float size;

        /// <summary>
        /// Tint of the water
        /// </summary>
        public MColor tint;
    }
}