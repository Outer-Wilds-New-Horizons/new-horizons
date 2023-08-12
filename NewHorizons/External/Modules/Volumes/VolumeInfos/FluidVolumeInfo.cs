using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class FluidVolumeInfo : PriorityVolumeInfo
    {
        /// <summary>
        /// Density of the fluid. The higher the density, the harder it is to go through this fluid.
        /// </summary>
        [DefaultValue(1.2f)] public float density = 1.2f;

        /// <summary>
        /// The type of fluid for this volume.
        /// </summary>
        public NHFluidType type;

        /// <summary>
        /// Should the player and rafts align to this fluid.
        /// </summary>
        [DefaultValue(true)] public bool alignmentFluid = true;

        /// <summary>
        /// Should the ship align to the fluid by rolling.
        /// </summary>
        public bool allowShipAutoroll;

        /// <summary>
        /// Disable this fluid volume immediately?
        /// </summary>
        public bool disableOnStart;
    }
}
