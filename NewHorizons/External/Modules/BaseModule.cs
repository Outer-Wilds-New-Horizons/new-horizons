using System.Runtime.Serialization;
using NewHorizons.Utility;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    public enum GravityFallOff
    {
        [EnumMember(Value = @"linear")]
        Linear = 0,
        
        [EnumMember(Value = @"inverseSquared")]
        InverseSquared = 1
    }
    
    [JsonObject]
    public class BaseModule
    {
        /// <summary>
        /// If the body should have a marker on the map screen.
        /// </summary>
        public bool HasMapMarker;
        
        /// <summary>
        /// The intensity of light the dark side of the body should have. Timber Hearth has `1.4` for reference
        /// </summary>
        public float AmbientLight;
        
        /// <summary>
        /// The acceleration due to gravity felt as the surfaceSize. Timber Hearth has 12 for reference
        /// </summary>
        public float SurfaceGravity;
        
        /// <summary>
        /// How gravity falls off with distance. Most planets use linear but the sun and some moons use inverseSquared.
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public GravityFallOff GravityFallOff = GravityFallOff.Linear;
        
        /// <summary>
        /// A scale height used for a number of things. Should be the approximate radius of the body.
        /// </summary>
        public float SurfaceSize;
        
        /// <summary>
        /// An override for the radius of the planet's gravitational sphere of influence. Optional
        /// </summary>
        public float SphereOfInfluence;
        
        /// <summary>
        /// Radius of a simple sphere used as the ground for the planet. If you want to use more complex terrain, leave this as 0.
        /// </summary>
        public float GroundSize;
        
        /// <summary>
        /// If you want the body to have a tail like the Interloper.
        /// </summary>
        public bool HasCometTail;
        
        /// <summary>
        /// If it has a comet tail, it'll be oriented according to these Euler angles.
        /// </summary>
        public MVector3 CometTailRotation;
        
        /// <summary>
        /// Allows the object to be targeted on the map.
        /// </summary>
        public bool HasReferenceFrame = true;
        
        /// <summary>
        /// Set this to true if you are replacing the sun with a different body. Only one object in a star system should ever have this set to true.
        /// </summary>
        public bool CenterOfSolarSystem = false;
        
        /// <summary>
        /// Radius of the cloaking field around the planet. It's a bit finicky so experiment with different values. If you don't want a cloak, leave this as 0.
        /// </summary>
        public float CloakRadius = 0f;
        
        /// <summary>
        /// Can this planet survive entering a star?
        /// </summary>
        public bool InvulnerableToSun;
        
        /// <summary>
        /// Do we show the minimap when walking around this planet?
        /// </summary>
        public bool ShowMinimap = true;

        #region Obsolete
        [System.Obsolete("IsSatellite is deprecated, please use ShowMinimap instead")] public bool IsSatellite;
        [System.Obsolete("BlackHoleSize is deprecated, please use SingularityModule instead")] public float BlackHoleSize;
        [System.Obsolete("LavaSize is deprecated, please use LavaModule instead")] public float LavaSize;
        [System.Obsolete("WaterTint is deprecated, please use WaterModule instead")] public float WaterSize;
        [System.Obsolete("WaterTint is deprecated, please use WaterModule instead")] public MColor WaterTint;
        [System.Obsolete("HasAmbientLight is deprecated, please use AmbientLight instead")] public bool HasAmbientLight;
        #endregion Obsolete
    }
}
