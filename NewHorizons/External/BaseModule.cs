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
        public MColor32 LightTint { get; set; }
        public float SurfaceGravity { get; set; }
        public float SurfaceSize { get; set; }
        public float WaterSize { get; set; }
        public float GroundSize { get; set; }
        public float BlackHoleSize { get; set; }
        public float LavaSize { get; set; }
    }
}
