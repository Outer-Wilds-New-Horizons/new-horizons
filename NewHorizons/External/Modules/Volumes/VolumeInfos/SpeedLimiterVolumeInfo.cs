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
        /// The distance from the outside of the volume that the limiter slows you down to max speed at.
        /// </summary>
        [DefaultValue(100f)]
        public float stoppingDistance = 100f;

        /// <summary>
        /// The maximum angle (in degrees) between the direction the incoming object is moving relative to the volume's center and the line from the object toward the center of the volume, within which the speed limiter will activate.
        /// </summary>
        [DefaultValue(60f)]
        public float maxEntryAngle = 60f;
    }
}
