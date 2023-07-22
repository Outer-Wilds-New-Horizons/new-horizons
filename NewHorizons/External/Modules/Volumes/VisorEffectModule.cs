using NewHorizons.External.Modules.Volumes.VolumeInfos;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewHorizons.External.Modules.Volumes
{
    [JsonObject]
    public class VisorEffectModule
    {
        /// <summary>
        /// Add visor frost effect volumes to this planet. This is the ghost matter effect.
        /// </summary>
        public FrostEffectVolumeInfo[] frostEffectVolumes;

        /// <summary>
        /// Add visor rain effect volumes to this planet. You can see this on Giant's Deep.
        /// </summary>
        public RainEffectVolumeInfo[] rainEffectVolumes;

        [JsonObject]
        public class FrostEffectVolumeInfo : PriorityVolumeInfo
        {
            /// <summary>
            /// The rate at which the frost effect will get stronger
            /// </summary>
            [DefaultValue(0.5f)]
            public float frostRate = 0.5f;

            /// <summary>
            /// The maximum strength of frost this volume can give
            /// </summary>
            [Range(0f, 1f)]
            [DefaultValue(0.91f)]
            public float maxFrost = 0.91f;
        }

        [JsonObject]
        public class RainEffectVolumeInfo : PriorityVolumeInfo
        {
            /// <summary>
            /// The rate at which the rain droplet effect will happen
            /// </summary>
            [DefaultValue(0.1f)]
            public float dropletRate = 10f;

            /// <summary>
            /// The rate at which the rain streak effect will happen
            /// </summary>
            [DefaultValue(1f)]
            public float streakRate = 1f;
        }
    }

}
