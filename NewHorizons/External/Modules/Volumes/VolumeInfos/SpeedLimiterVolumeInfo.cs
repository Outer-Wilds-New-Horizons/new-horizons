using Newtonsoft.Json;
using System.ComponentModel;
using UnityEngine;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class SpeedLimiterVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The speed the volume will slow you down to when you enter it.
        /// </summary>
        [DefaultValue(10f)]
        public float maxSpeed = 10f;

        /// <summary>
        /// 
        /// </summary>
        [DefaultValue(100f)]
        public float stoppingDistance = 100f;

        /// <summary>
        /// 
        /// </summary>
        [DefaultValue(60f)]
        public float maxEntryAngle = 60f;
    }
}
