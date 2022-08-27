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
        public float size;

        /// <summary>
        /// Tint of the water
        /// </summary>
        public MColor tint;

        /// <summary>
        /// Size of the interior. Useful for if you want the core of the planet to have no water.
        /// </summary>
        public float interiorSize;
    }
}