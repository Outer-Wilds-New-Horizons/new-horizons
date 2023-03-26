using NewHorizons.External.Modules.VariableSize;
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class CometTailModule : VariableSizeModule
    {
        /// <summary>
        /// Manually sets the local rotation
        /// </summary>
        public MVector3 rotationOverride;

        /// <summary>
        /// Inner radius of the comet tail, defaults to match surfaceSize
        /// </summary>
        public float? innerRadius;

        /// <summary>
        /// The body that the comet tail should always point away from
        /// </summary>
        public string primaryBody;

        /// <summary>
        /// Colour of the dust tail (the shorter part)
        /// </summary>
        public MColor dustTint;

        /// <summary>
        /// Colour of the gas tail (the longer part)
        /// </summary>
        public MColor gasTint;
    }
}
