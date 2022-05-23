using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class RingModule : VariableSizeModule
    {
        /// <summary>
        /// Fluid type for sounds/effects when colliding with this ring.
        /// </summary>
        public CloudFluidType fluidType = CloudFluidType.None;

        /// <summary>
        /// Angle between the rings and the equatorial plane of the planet.
        /// </summary>
        public float inclination;

        /// <summary>
        /// Inner radius of the disk
        /// </summary>
        [Range(0, double.MaxValue)] public float innerRadius;

        /// <summary>
        /// Angle defining the point where the rings rise up from the planet's equatorial plane if inclination is nonzero.
        /// </summary>
        public float longitudeOfAscendingNode;

        /// <summary>
        /// Outer radius of the disk
        /// </summary>
        [Range(0, double.MaxValue)] public float outerRadius;

        /// <summary>
        /// Allows the rings to rotate.
        /// </summary>
        public float rotationSpeed;

        /// <summary>
        /// Relative filepath to the texture used for the rings.
        /// </summary>
        public string texture;

        /// <summary>
        /// Should this ring be unlit?
        /// </summary>
        public bool unlit;
    }
}