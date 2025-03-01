using NewHorizons.External.Modules.Volumes.VolumeInfos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Volumes
{
    [JsonObject]
    public class ForceModule
    {
        /// <summary>
        /// Applies a constant force along the volume's XZ plane towards the volume's center. Affects alignment.
        /// </summary>
        public CylindricalForceVolumeInfo[] cylindricalVolumes;

        /// <summary>
        /// Applies a constant force in the direction of the volume's Y axis. May affect alignment.
        /// </summary>
        public DirectionalForceVolumeInfo[] directionalVolumes;

        /// <summary>
        /// Applies planet-like gravity towards the volume's center with falloff by distance. May affect alignment.
        /// For actual planetary body gravity, use the properties in the Base module.
        /// </summary>
        public GravityVolumeInfo[] gravityVolumes;

        /// <summary>
        /// Applies a constant force towards the volume's center. Affects alignment.
        /// </summary>
        public PolarForceVolumeInfo[] polarVolumes;

        /// <summary>
        /// Applies a force towards the volume's center with falloff by distance. Affects alignment.
        /// </summary>
        public RadialForceVolumeInfo[] radialVolumes;
    }
}
