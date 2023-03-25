using NewHorizons.External.Modules.SerializableData;
using Newtonsoft.Json;
using System.ComponentModel;

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
        /// Density of the water sphere. The higher the density, the harder it is to go through this fluid.
        /// </summary>
        [DefaultValue(1.2f)] public float density = 1.2f;

        /// <summary>
        /// Buoyancy density of the water sphere
        /// </summary>
        [DefaultValue(1f)] public float buoyancy = 1f;

        /// <summary>
        /// Tint of the water
        /// </summary>
        public MColor tint;
    }
}