using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Orbital
{
    public class Gravity
    {
        public int Power { get; private set; }
        public float Mass { get; private set; }

        public Gravity(float mass, int power)
        {
            Power = power;
            Mass = mass;
        }

        public Gravity(BaseModule module) 
        {
            var surfaceAcceleration = module.SurfaceGravity;
            var upperSurfaceRadius = module.SurfaceSize;
            int falloffExponent = module.GravityFallOff.ToUpper().Equals("LINEAR") ? 1 : 2;

            Mass = surfaceAcceleration * Mathf.Pow(upperSurfaceRadius, falloffExponent) / GravityVolume.GRAVITATIONAL_CONSTANT;
            Power = falloffExponent;
        }

        public Gravity(GravityVolume gv)
        {
            Mass = gv._gravitationalMass;
            Power = gv._falloffType == GravityVolume.FalloffType.linear ? 1 : 2;
        }
    }
}
