using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class BaseModule : Module
    {
        public bool HasMapMarker { get; set; }
        public bool HasAmbientLight { get; set; }
        public float SurfaceGravity { get; set; }
        public string GravityFallOff { get; set; } = "linear";
        public float SurfaceSize { get; set; }
        public float WaterSize { get; set; }
        public float GroundSize { get; set; }
        public float LavaSize { get; set; }
        public bool HasCometTail { get; set; }
        public bool HasReferenceFrame { get; set; } = true;
        public bool CenterOfSolarSystem { get; set; } = false;
        public bool IsSatellite { get; set; }

        // Old, see SingularityModule instead
        public float BlackHoleSize { get; set; }
    }
}
