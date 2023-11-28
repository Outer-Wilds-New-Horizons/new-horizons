using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Modules
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GravityFallOff
    {
        [EnumMember(Value = @"linear")] Linear = 0,

        [EnumMember(Value = @"inverseSquared")]
        InverseSquared = 1
    }

    [JsonObject]
    public class BaseModule
    {
        /// <summary>
        /// Set this to true if you are replacing the sun with a different body. Only one object in a star system should ever
        /// have this set to true.
        /// </summary>
        public bool centerOfSolarSystem;

        /// <summary>
        /// How gravity falls off with distance. Most planets use linear but the sun and some moons use inverseSquared.
        /// </summary>
        [DefaultValue("linear")] public GravityFallOff gravityFallOff = GravityFallOff.Linear;

        /// <summary>
        /// Radius of a simple sphere used as the ground for the planet. If you want to use more complex terrain, leave this as
        /// 0.
        /// </summary>
        public float groundSize;

        /// <summary>
        /// If the body should have a marker on the map screen.
        /// </summary>
        public bool hasMapMarker;

        /// <summary>
        /// Can this planet survive entering a star?
        /// </summary>
        public bool invulnerableToSun;

        /// <summary>
        /// Do we show the minimap when walking around this planet?
        /// </summary>
        [DefaultValue(true)] public bool showMinimap = true;

        /// <summary>
        /// An override for the radius of the planet's gravitational sphere of influence. Optional
        /// </summary>
        public float soiOverride;

        /// <summary>
        /// The acceleration due to gravity felt as the surfaceSize. Timber Hearth has 12 for reference
        /// </summary>
        public float surfaceGravity;

        /// <summary>
        /// A scale height used for a number of things. Should be the approximate radius of the body.
        /// </summary>
        public float surfaceSize;

        /// <summary>
        /// Optional. You can force this planet's gravity to be felt over other gravity/zero-gravity sources by increasing this number.
        /// </summary>
        [DefaultValue(0)] public int gravityVolumePriority = 0;

        /// <summary>
        /// Apply physics to this planet when you bump into it. Will have a spherical collider the size of surfaceSize. 
        /// For custom colliders they have to all be convex and you can leave surface size as 0.
        /// This is meant for stuff like satellites which are relatively simple and can be de-orbited.
        /// If you are using an orbit line but a tracking line, it will be removed when the planet is bumped in to.
        /// </summary>
        public bool pushable;

        /// <summary>
        /// Set this to true to have no proxy be generated for this planet. 
        /// This is a small representation of the planet that appears when it is outside of the regular Unity camera range.
        /// </summary>
        public bool hideProxy;

        #region Obsolete

        [Obsolete("IsSatellite is deprecated, please use ShowMinimap instead")]
        public bool isSatellite;

        [Obsolete("BlackHoleSize is deprecated, please use SingularityModule instead")]
        public float blackHoleSize;

        [Obsolete("LavaSize is deprecated, please use LavaModule instead")]
        public float lavaSize;

        [Obsolete("WaterTint is deprecated, please use WaterModule instead")]
        public float waterSize;

        [Obsolete("WaterTint is deprecated, please use WaterModule instead")]
        public MColor waterTint;

        [Obsolete("HasAmbientLight is deprecated, please use AmbientLightModule instead")]
        public bool hasAmbientLight;

        [Obsolete("AmbientLight is deprecated, please use AmbientLightModule instead")]
        public float ambientLight;

        [Obsolete("HasReferenceFrame is deprecated, please use ReferenceModule instead")]
        [DefaultValue(true)] public bool hasReferenceFrame = true;

        [Obsolete("CloakRadius is deprecated, please use CloakModule instead")]
        public float cloakRadius;

        [Obsolete("SphereOfInfluence is deprecated, please use soiOverride instead")]
        public float sphereOfInfluence;

        [Obsolete("zeroGravityRadius is deprecated, please use Volumes->ZeroGravityVolumes instead")]
        public float zeroGravityRadius;

        [Obsolete("hasCometTail is deprecated, please use CometTail instead")]
        public bool hasCometTail;

        [Obsolete("cometTailRotation is deprecated, please use CometTail->rotationOverride instead")]
        public MVector3 cometTailRotation;

        #endregion Obsolete
    }
}