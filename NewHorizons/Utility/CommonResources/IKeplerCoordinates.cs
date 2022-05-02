using PacificEngine.OW_CommonResources.Geometry.Orbits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Utility.CommonResources
{
    public interface IKeplerCoordinates
    {
        float Inclination { get; set; }
        int SemiMajorAxis { get; set; }
        float LongitudeOfAscendingNode { get; set; }
        float Eccentricity { get; set; }
        float ArgumentOfPeriapsis { get; set; }
        float TrueAnomaly { get; set; }

        KeplerCoordinates GetKeplerCoords();
    }
}
