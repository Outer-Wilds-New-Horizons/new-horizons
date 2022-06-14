using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using NewHorizons.Utility;
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
        /// The intensity of light the dark side of the body should have. Timber Hearth has `1.4` for reference
        /// </summary>
        public float ambientLight;

        /// <summary>
        /// Set this to true if you are replacing the sun with a different body. Only one object in a star system should ever
        /// have this set to true.
        /// </summary>
        public bool centerOfSolarSystem;

        /// <summary>
        /// If it has a comet tail, it'll be oriented according to these Euler angles.
        /// </summary>
        public MVector3 cometTailRotation;

        /// <summary>
        /// How gravity falls off with distance. Most planets use linear but the sun and some moons use inverseSquared.
        /// </summary>
        public GravityFallOff gravityFallOff = GravityFallOff.Linear;

        /// <summary>
        /// Radius of a simple sphere used as the ground for the planet. If you want to use more complex terrain, leave this as
        /// 0.
        /// </summary>
        public float groundSize;

        /// <summary>
        /// If you want the body to have a tail like the Interloper.
        /// </summary>
        public bool hasCometTail;

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
        public float sphereOfInfluence;

        /// <summary>
        /// The acceleration due to gravity felt as the surfaceSize. Timber Hearth has 12 for reference
        /// </summary>
        public float surfaceGravity;

        /// <summary>
        /// A scale height used for a number of things. Should be the approximate radius of the body.
        /// </summary>
        public float surfaceSize;

        /// <summary>
        /// Radius of the zero gravity volume. This will make it so no gravity from any planet will affect you. Useful for satellites.
        /// </summary>
        public float zeroGravityRadius;

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

        [Obsolete("HasAmbientLight is deprecated, please use AmbientLight instead")]
        public bool hasAmbientLight;

        [Obsolete("HasReferenceFrame is deprecated, please use ReferenceModule instead")]
        [DefaultValue(true)] public bool hasReferenceFrame = true;

        [Obsolete("CloakRadius is deprecated, please use CloakModule instead")]
        public float cloakRadius;

        #endregion Obsolete
    }
}