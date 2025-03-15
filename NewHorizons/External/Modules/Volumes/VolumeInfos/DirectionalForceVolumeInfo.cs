using NewHorizons.External.SerializableData;
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
    public class DirectionalForceVolumeInfo : ForceVolumeInfo
    {
        /// <summary>
        /// The direction of the force applied by this volume. Defaults to up (0, 1, 0).
        /// </summary>
        public MVector3 normal;

        /// <summary>
        /// Whether this force volume affects alignment. Defaults to true.
        /// </summary>
        [DefaultValue(true)] public bool affectsAlignment = true;

        /// <summary>
        /// Whether the force applied by this volume takes the centripetal force of the volume's parent body into account. Defaults to false.
        /// </summary>
        public bool offsetCentripetalForce;

        /// <summary>
        /// Whether to play the gravity crystal audio when the player is in this volume.
        /// </summary>
        public bool playGravityCrystalAudio;
    }
}
