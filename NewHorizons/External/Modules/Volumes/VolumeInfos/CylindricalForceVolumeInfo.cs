using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class CylindricalForceVolumeInfo : ForceVolumeInfo
    {
        /// <summary>
        /// The direction that the force applied by this volume will be perpendicular to. Defaults to up (0, 1, 0).
        /// </summary>
        public MVector3 normal;

        /// <summary>
        /// Whether to play the gravity crystal audio when the player is in this volume.
        /// </summary>
        public bool playGravityCrystalAudio;
    }
}
