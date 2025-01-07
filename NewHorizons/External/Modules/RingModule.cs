using NewHorizons.External.Modules.VariableSize;
using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RingTransparencyType
    {
        [EnumMember(Value = @"fade")] Fade = 0,

        [EnumMember(Value = @"alphaClip")] AlphaClip = 1,
    }

    [JsonObject]
    public class RingModule
    {
        /// <summary>
        /// Should the texture's transparency cause the ring to fade out, or be opaque above 0.5 and invisible below?
        /// Warning! Fade transparency does not support shadows!
        /// </summary>
        [DefaultValue("alphaClip")] public RingTransparencyType transparencyType = RingTransparencyType.AlphaClip;

        /// <summary>
        /// Fluid type for sounds/effects when colliding with this ring.
        /// </summary>
        public NHFluidType fluidType = NHFluidType.NONE;

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
        /// Rotation speed, in degrees per second.
        /// </summary>
        public float rotationSpeed;

        /// <summary>
        /// Relative filepath to the main texture. Can be either a full image, or a one pixel slice.
        /// </summary>
        public string texture;

        /// <summary>
        /// Should this ring be unlit?
        /// </summary>
        [DefaultValue(false)] public bool unlit;

        /// <summary>
        /// Modifies the transparency with noise, giving it a grainy appearance. Works well with alpha clip transparency.
        /// </summary>
        public bool? useNoise;

        /// <summary>
        /// Size in meters of each noise pixel.
        /// </summary>
        [DefaultValue(1f)] public float noiseScale = 1f;

        /// <summary>
        /// Rotation speed of the noise, in degrees per second. Rotating noise will multiply with still noise, giving it a flowing appearance.
        /// </summary>
        [DefaultValue(0.5f)] public float noiseRotationSpeed = 0.5f;

        /// <summary>
        /// How much light shines through to the underside of the ring.
        /// </summary>
        [DefaultValue(0.1f)] public float translucency = 0.1f;

        /// <summary>
        /// Adds a translucent glow when viewing the sun through the underside of the ring.
        /// </summary>
        [DefaultValue(true)] public bool translucentGlow = true;

        /// <summary>
        /// Relative filepath to a texture for smoothness and metallic, using the alpha and red channels respectively. Optional.
        /// </summary>
        public string smoothnessMap;

        /// <summary>
        /// Relative filepath to a normal (aka bump) texture. Optional. Does not work with a one pixel slice.
        /// </summary>
        public string normalMap;

        /// <summary>
        /// Relative filepath to an emissive texture. Optional.
        /// </summary>
        public string emissionMap;

        #region Obsolete
        [Obsolete("curve is deprecated, please use scaleCurve instead")]
        public TimeValuePair[] curve;
        #endregion

        /// <summary>
        /// Scale rings over time. Optional. Value between 0-1, time is in minutes.
        /// </summary>
        public TimeValuePair[] scaleCurve;

        /// <summary>
        /// Fade rings in/out over time. Optional. Value between 0-1, time is in minutes.
        /// </summary>
        public TimeValuePair[] opacityCurve;

        /// <summary>
        /// An optional rename of this object
        /// </summary>
        public string rename;
    }
}