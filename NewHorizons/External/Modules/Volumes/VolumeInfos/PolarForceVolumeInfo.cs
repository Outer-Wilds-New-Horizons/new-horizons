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
    public class PolarForceVolumeInfo : ForceVolumeInfo
    {
        /// <summary>
        /// Tangential mode only. The force applied by this volume will be perpendicular to this direction and the direction to the other body. Defaults to up (0, 1, 0).
        /// </summary>
        public MVector3 normal;
        /// <summary>
        /// Enables tangential mode. The force applied by this volume will be perpendicular to the normal and the direction to the other body. Defaults to false.
        /// </summary>
        public bool tangential;
    }
}
