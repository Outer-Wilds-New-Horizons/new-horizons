using Marshmallow.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Marshmallow.External
{
    public class PlanetConfig : IPlanetConfig
    {
        public string Name { get; set; }
        public MVector3 Position { get; set; }
        public int OrbitAngle { get; set; }
        public string PrimaryBody { get; set; }
        public bool IsMoon { get; set; }
        //public bool HasSpawnPoint { get; set; }
        public bool HasClouds { get; set; }
        public float TopCloudSize { get; set; }
        public float BottomCloudSize { get; set; }
        public MColor32 TopCloudTint { get; set; }
        public MColor32 BottomCloudTint { get; set; }
        public bool HasWater { get; set; }
        public float WaterSize { get; set; }
        public bool HasRain { get; set; }
        public bool HasGravity { get; set; }
        public float SurfaceAcceleration { get; set; }
        public bool HasMapMarker { get; set; }
        public bool HasFog { get; set; }
        public MColor32 FogTint { get; set; }
        public float FogDensity { get; set; }
        public float GroundSize { get; set; }
        public bool IsTidallyLocked { get; set; }
        public MColor32 LightTint { get; set; }
	}
}
