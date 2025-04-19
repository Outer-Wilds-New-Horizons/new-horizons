using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class GravityVolumeInfo : ForceVolumeInfo
    {
        /// <summary>
        /// The upper bounds of the volume's "surface". Above this radius, the force applied by this volume will have falloff applied.
        /// </summary>
        public float upperRadius;

        /// <summary>
        /// The lower bounds of the volume's "surface". Above this radius and below the `upperRadius`, the force applied by this volume will be constant. Defaults to 0.
        /// </summary>
        [DefaultValue(0f)] public float lowerRadius;

        /// <summary>
        /// The volume's force will decrease linearly from `force` to `minForce` as distance decreases from `lowerRadius` to `minRadius`. Defaults to 0.
        /// </summary>
        [DefaultValue(0f)] public float minRadius;

        /// <summary>
        /// The minimum force applied by this volume between `lowerRadius` and `minRadius`. Defaults to 0.
        /// </summary>
        [DefaultValue(0f)] public float minForce;

        /// <summary>
        /// How the force falls off with distance. Most planets use linear but the sun and some moons use inverseSquared.
        /// </summary>
        [DefaultValue("linear")] public GravityFallOff fallOff = GravityFallOff.Linear;

        /// <summary>
        /// The radius where objects will be aligned to the volume's force. Defaults to 1.5x the `upperRadius`. Set to 0 to disable alignment.
        /// </summary>
        public float? alignmentRadius;
    }
}
