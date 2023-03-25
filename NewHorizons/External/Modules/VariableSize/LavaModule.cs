using NewHorizons.External.SerializableData;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class LavaModule : VariableSizeModule
    {
        /// <summary>
        /// Size of the lava sphere
        /// </summary>
        public float size;

        /// <summary>
        /// Tint of the lava
        /// </summary>
        public MColor tint;
    }
}