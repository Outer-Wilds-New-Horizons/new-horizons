using Newtonsoft.Json;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class RingModule : VariableSizeModule
    {
        /// <summary>
        /// Inner radius of the disk
        /// </summary>
        public float InnerRadius;
        
        /// <summary>
        /// Outer radius of the disk
        /// </summary>
        public float OuterRadius;
        
        /// <summary>
        /// Angle between the rings and the equatorial plane of the planet.
        /// </summary>
        public float Inclination;
        
        /// <summary>
        /// Angle defining the point where the rings rise up from the planet's equatorial plane if inclination is nonzero.
        /// </summary>
        public float LongitudeOfAscendingNode;
        
        /// <summary>
        /// Relative filepath to the texture used for the rings.
        /// </summary>
        public string Texture;
        
        /// <summary>
        /// Should this ring be unlit?
        /// </summary>
        public bool Unlit;
        
        /// <summary>
        /// Allows the rings to rotate.
        /// </summary>
        public float RotationSpeed;

        /// <summary>
        /// Fluid type for sounds/effects when colliding with this ring.
        /// </summary>
        public CloudFluidType? FluidType = CloudFluidType.None;
    }
}
