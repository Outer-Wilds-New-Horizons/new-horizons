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
        public string name { get; set; }
        public MVector3 position { get; set; }
        public int orbitAngle { get; set; }
        public string primaryBody { get; set; }
        public bool hasSpawnPoint { get; set; }
        public bool hasClouds { get; set; }
        public float topCloudSize { get; set; }
        public float bottomCloudSize { get; set; }
        public MColor32 topCloudTint { get; set; }
        public MColor32 bottomCloudTint { get; set; }
        public bool hasWater { get; set; }
        public float waterSize { get; set; }
        public bool hasRain { get; set; }
        public bool hasGravity { get; set; }
        public float surfaceAcceleration { get; set; }
        public bool hasMapMarker { get; set; }
        public bool hasFog { get; set; }
        public MColor32 fogTint { get; set; }
        public float fogDensity { get; set; }
        public float groundSize { get; set; }
	}
}
