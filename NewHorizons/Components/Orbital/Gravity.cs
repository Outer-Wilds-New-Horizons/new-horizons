using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Components.Orbital
{
    public class Gravity
    {
        public int Power { get; }
        public float Mass { get; }

        public Gravity(float mass, int power)
        {
            Power = power;
            Mass = mass;
        }

        public Gravity(BaseModule module)
        {
            var surfaceAcceleration = module.surfaceGravity;
            var upperSurfaceRadius = module.surfaceSize;
            var falloffExponent = module.gravityFallOff == GravityFallOff.Linear ? 1 : 2;

            Mass = surfaceAcceleration * Mathf.Pow(upperSurfaceRadius, falloffExponent) /
                   GravityVolume.GRAVITATIONAL_CONSTANT;
            Power = falloffExponent;
        }

        public Gravity(GravityVolume gv)
        {
            if (gv == null)
            {
                Mass = 0;
                Power = 2;
            }
            else
            {
                Power = gv._falloffType == GravityVolume.FalloffType.linear ? 1 : 2;
                Mass = gv._surfaceAcceleration * Mathf.Pow(gv._upperSurfaceRadius, Power) /
                       GravityVolume.GRAVITATIONAL_CONSTANT;
            }
        }
    }
}