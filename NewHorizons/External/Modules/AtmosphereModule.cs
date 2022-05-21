using System.Runtime.Serialization;
using NewHorizons.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Modules
{
    public enum CloudFluidType
    {
        [EnumMember(Value = @"NONE")]
        None = 0,
            
        [EnumMember(Value = @"WATER")]
        Water = 1,
            
        [EnumMember(Value = @"CLOUD")]
        Cloud = 2,
            
        [EnumMember(Value = @"SAND")]
        Sand = 3,
            
        [EnumMember(Value = @"PLASMA")]
        Plasma = 4
    }
    
    [JsonObject]
    public class AtmosphereModule
    {
        /// <summary>
        /// Scale height of the atmosphere
        /// </summary>
        public float Size;
        
        /// <summary>
        /// Colour of atmospheric shader on the planet.
        /// </summary>
        public MColor AtmosphereTint;

        /// <summary>
        /// Colour of fog on the planet, if you put fog.
        /// </summary>
        public MColor FogTint;

        /// <summary>
        /// How dense the fog is, if you put fog.
        /// </summary>
        // FIXME: Min & Max Needed!
        public float FogDensity;
        
        /// <summary>
        /// Radius of fog sphere, independent of the atmosphere. This has to be set for there to be fog.
        /// </summary>
        public float FogSize;
        
        /// <summary>
        /// Does this planet have rain?
        /// </summary>
        public bool HasRain;
        
        /// <summary>
        /// Does this planet have snow?
        /// </summary>
        public bool HasSnow;
        
        /// <summary>
        /// Lets you survive on the planet without a suit.
        /// </summary>
        public bool HasOxygen;
        
        /// <summary>
        /// Whether we use an atmospheric shader on the planet. Doesn't affect clouds, fog, rain, snow, oxygen, etc. Purely visual.
        /// </summary>
        public bool UseAtmosphereShader;
        
        /// <summary>
        /// Describes the clouds in the atmosphere
        /// </summary>
        public CloudInfo Clouds;


        #region Obsolete
        [System.Obsolete("CloudTint is deprecated, please use CloudInfo instead")] public MColor CloudTint;
        [System.Obsolete("CloudTint is deprecated, please use CloudInfo instead")] public string Cloud;
        [System.Obsolete("CloudCap is deprecated, please use CloudInfo instead")] public string CloudCap;
        [System.Obsolete("CloudRamp is deprecated, please use CloudInfo instead")] public string CloudRamp;
        [System.Obsolete("CloudFluidType is deprecated, please use CloudInfo instead")] 
        [JsonConverter(typeof(StringEnumConverter))]
        public CloudFluidType? FluidType;
        [System.Obsolete("UseBasicCloudShader is deprecated, please use CloudInfo instead")] public bool UseBasicCloudShader;
        [System.Obsolete("ShadowsOnClouds is deprecated, please use CloudInfo instead")] public bool ShadowsOnClouds = true;
        [System.Obsolete("HasAtmosphere is deprecated, please use UseAtmosphereShader instead")] public bool HasAtmosphere;
        #endregion Obsolete

        public class AirInfo
        {
            public float Scale;
            public bool HasOxygen;
            public bool IsRaining;
            public bool IsSnowing;
        }

        [JsonObject]
        public class CloudInfo
        {
            /// <summary>
            /// Radius from the center to the outer layer of the clouds.
            /// </summary>
            public float OuterCloudRadius;
            
            /// <summary>
            /// Radius from the center to the inner layer of the clouds.
            /// </summary>
            public float InnerCloudRadius;
            
            /// <summary>
            /// Colour of the inner cloud layer.
            /// </summary>
            public MColor Tint;
            
            /// <summary>
            /// Relative filepath to the cloud texture, if the planet has clouds.
            /// </summary>
            public string TexturePath;
            
            /// <summary>
            /// Relative filepath to the cloud cap texture, if the planet has clouds.
            /// </summary>
            public string CapPath;
            
            /// <summary>
            /// Relative filepath to the cloud ramp texture, if the planet has clouds. If you don't put anything here it will be auto-generated.
            /// </summary>
            public string RampPath;

            /// <summary>
            /// Fluid type for sounds/effects when colliding with this cloud.
            /// </summary>
            [JsonConverter(typeof(StringEnumConverter))]
            public CloudFluidType? FluidType = CloudFluidType.Cloud;
            
            /// <summary>
            /// Set to `false` in order to use Giant's deep shader. Set to `true` to just apply the cloud texture as is.
            /// </summary>
            public bool UseBasicCloudShader;
            
            /// <summary>
            /// If the top layer shouldn't have shadows. Set to true if you're making a brown dwarf for example.
            /// </summary>
            public bool Unlit;
            
            /// <summary>
            /// Add lightning to this planet like on Giant's Deep.
            /// </summary>
            public bool HasLightning;
            
            /// <summary>
            /// Colour gradient of the lightning, time is in seconds.
            /// </summary>
            public MGradient[] LightningGradient;
        }
    }
}
