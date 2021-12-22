using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewHorizons.Utility;

namespace NewHorizons.OrbitalPhysics
{
    public class Gravity
    {
        public float Exponent { get; }
        public float Mass { get; }

        public Gravity(float exponent, float mass)
        {
            Exponent = exponent;
            Mass = mass;
        }

        public Gravity(GravityVolume gv)
        {
            Exponent = gv.GetFalloffExponent();
            Mass = gv.GetStandardGravitationalParameter() / GravityVolume.GRAVITATIONAL_CONSTANT;
        }
    }
}
