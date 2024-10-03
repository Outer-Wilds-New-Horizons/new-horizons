using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using NewHorizons.External.SerializableData;
using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Modules
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CloudPrefabType
    {
        [EnumMember(Value = @"giantsDeep")] GiantsDeep = 0,

        [EnumMember(Value = @"quantumMoon")] QuantumMoon = 1,

        [EnumMember(Value = @"basic")] Basic = 2,

        [EnumMember(Value = @"transparent")] Transparent = 3,
    }

    [JsonObject]
    public class AtmosphereModule
    {
        /// <summary>
        /// Colour of atmospheric shader on the planet.
        /// </summary>
        public MColor atmosphereTint;

        /// <summary>
        /// How intense should the sun appear in the sky.
        /// Also affects general atmosphere brightness.
        /// Default value of 1 matches Timber Hearth.
        /// If not set, this will be determined based off the distance to the sun.
        /// </summary>
        [Range(0f, double.MaxValue)]
        public float atmosphereSunIntensity;

        /// <summary>
        /// Describes the clouds in the atmosphere
        /// </summary>
        public CloudInfo clouds;

        /// <summary>
        /// How dense the fog is, if you put fog.
        /// </summary>
        [Range(0f, double.MaxValue)] public float fogDensity;

        /// <summary>
        /// Radius of fog sphere, independent of the atmosphere. This has to be set for there to be fog.
        /// </summary>
        [Range(0f, double.MaxValue)] public float fogSize;

        /// <summary>
        /// Colour of fog on the planet, if you put fog.
        /// </summary>
        public MColor fogTint;

        /// <summary>
        /// Relative filepath to the fog color ramp texture, if you put fog.
        /// x axis is angle to sun (left at midnight, right at noon), y axis is distance to camera (close at bottom, far at top).
        /// </summary>
        public string fogRampPath;

        /// <summary>
        /// Lets you survive on the planet without a suit.
        /// </summary>
        public bool hasOxygen;

        /// <summary>
        /// Does this planet have trees? This will change the notification from "Oxygen tank refilled" to "Trees detected, oxygen tank refilled".
        /// </summary>
        public bool hasTrees;

        /// <summary>
        /// Does this planet have rain? 
        /// This is equivalent to effects of setting a rain particle/vection field, rain audio volume, and visor effect volume, combined for convenience.
        /// For more control over the rain, use those individual components.
        /// </summary>
        public bool hasRain;

        /// <summary>
        /// Scale height of the atmosphere
        /// </summary>
        public float size;

        /// <summary>
        /// Whether we use an atmospheric shader on the planet. Doesn't affect clouds, fog, rain, snow, oxygen, etc. Purely
        /// visual.
        /// </summary>
        public bool useAtmosphereShader;

        /// <summary>
        /// Whether this atmosphere will have flames appear when your ship goes a certain speed.
        /// </summary>
        [DefaultValue(true)] public bool hasShockLayer = true;

        /// <summary>
        /// Minimum speed that your ship can go in the atmosphere where flames will appear.
        /// </summary>
        [DefaultValue(100f)] public float minShockSpeed = 100f;

        /// <summary>
        /// Maximum speed that your ship can go in the atmosphere where flames will appear at their brightest.
        /// </summary>
        [DefaultValue(300f)] public float maxShockSpeed = 300f;

        /// <summary>
        /// Will the ship automatically try to orient itself to face upwards while in this volume?
        /// </summary>
        [DefaultValue(true)]
        public bool allowShipAutoroll = true;

        [JsonObject]
        public class CloudInfo
        {
            /// <summary>
            /// Should these clouds be based on Giant's Deep's banded clouds, or the Quantum Moon's non-banded clouds?
            /// </summary>
            public CloudPrefabType cloudsPrefab;

            /// <summary>
            /// Relative filepath to the cloud cap texture, if the planet has clouds.
            /// </summary>
            public string capPath;

            /// <summary>
            /// Fluid type for sounds/effects when colliding with this cloud.
            /// </summary>
            [DefaultValue("cloud")] public NHFluidType fluidType = NHFluidType.CLOUD;

            /// <summary>
            /// Add lightning to this planet like on Giant's Deep.
            /// </summary>
            public bool hasLightning;

            /// <summary>
            /// Radius from the center to the inner layer of the clouds.
            /// </summary>
            public float innerCloudRadius;

            /// <summary>
            /// Colour gradient of the lightning, time is in seconds.
            /// </summary>
            public MGradient[] lightningGradient;

            /// <summary>
            /// Radius from the center to the outer layer of the clouds.
            /// </summary>
            public float outerCloudRadius;

            /// <summary>
            /// Relative filepath to the cloud ramp texture, if the planet has clouds. If you don't put anything here it will be
            /// auto-generated.
            /// </summary>
            public string rampPath;

            /// <summary>
            /// Relative filepath to the cloud texture, if the planet has clouds.
            /// </summary>
            public string texturePath;

            /// <summary>
            /// Colour of the inner cloud layer.
            /// </summary>
            public MColor tint;

            /// <summary>
            /// If the top layer shouldn't have shadows. Set to true if you're making a brown dwarf for example.
            /// </summary>
            public bool unlit;

            /// <summary>
            /// How fast the clouds will rotate relative to the planet in degrees per second.
            /// </summary>
            [DefaultValue(0f)] public float rotationSpeed = 0f;

            #region Obsolete

            /// <summary>
            /// Set to `false` in order to use Giant's Deep's shader. Set to `true` to just apply the cloud texture as is.
            /// </summary>
            [Obsolete("useBasicCloudShader is deprecated, please use cloudsPrefab=\"basic\" instead")]
            public bool useBasicCloudShader;

            #endregion Obsolete

        }


        #region Obsolete
        [Obsolete("HasSnow is deprecated, please use ParticleFields instead")]
        public bool hasSnow;

        [Obsolete("CloudTint is deprecated, please use CloudInfo instead")]
        public MColor cloudTint;

        [Obsolete("CloudTint is deprecated, please use CloudInfo instead")]
        public string cloud;

        [Obsolete("CloudCap is deprecated, please use CloudInfo instead")]
        public string cloudCap;

        [Obsolete("CloudRamp is deprecated, please use CloudInfo instead")]
        public string cloudRamp;

        [Obsolete("FluidType is deprecated, please use CloudInfo instead")]
        public NHFluidType fluidType;

        [Obsolete("UseBasicCloudShader is deprecated, please use CloudInfo instead")]
        public bool useBasicCloudShader;

        [DefaultValue(true)]
        [Obsolete("ShadowsOnClouds is deprecated, please use CloudInfo instead")]
        public bool shadowsOnClouds = true;

        [Obsolete("HasAtmosphere is deprecated, please use UseAtmosphereShader instead")]
        public bool hasAtmosphere;

        #endregion Obsolete
    }
}