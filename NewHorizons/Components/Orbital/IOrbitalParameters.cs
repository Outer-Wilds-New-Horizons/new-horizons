using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Components.Orbital
{
    public interface IOrbitalParameters
    {
        float Inclination { get; set; }
        float SemiMajorAxis { get; set; }
        float LongitudeOfAscendingNode { get; set; }
        float Eccentricity { get; set; }
        float ArgumentOfPeriapsis { get; set; }
        float TrueAnomaly { get; set; }

        OrbitalParameters GetOrbitalParameters(Gravity primaryGravity, Gravity secondaryGravity);
    }
}
