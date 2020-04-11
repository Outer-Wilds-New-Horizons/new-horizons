using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Marshmallow
{
    public class PlanetStructure
    {
        public string name;

        public AstroObject primaryBody;
        public AstroObject.Type aoType;
        public AstroObject.Name aoName;

        public Vector3 position;

        public bool makeSpawnPoint = false;
        
        public bool hasClouds = false;
        public float? topCloudSize = null;
        public float? bottomCloudSize = null;
        public Color? cloudTint = null;

        public bool hasWater = false;
        public float? waterSize = null;

        public bool hasRain = false;

        public bool hasGravity = false;
        public float surfaceAccel = 1f;

        public bool hasMapMarker = false;

        public bool hasFog = false;
        public Color fogTint = Color.white;
        public float fogDensity = 0.3f;

        public bool hasOrbit = false;
    }
}
