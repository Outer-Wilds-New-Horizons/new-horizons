using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.OrbitalPhysics
{
    public class ParameterizedOrbitLine : EllipticOrbitLine
	{
		public ParameterizedAstroObject astroObject;

        public override void Start()
        {
            base.Start();

            astroObject = _astroObject as ParameterizedAstroObject;

            var periapsis = OrbitalHelper.GetPositionFromEccentricAnomaly(astroObject.Eccentricity, astroObject.SemiMajorAxis, astroObject.Inclination + 90, astroObject.ArgumentOfPeriapsis, astroObject.LongitudeOfAscendingNode, 0f);
            var semiMinorDecending = OrbitalHelper.GetPositionFromEccentricAnomaly(astroObject.Eccentricity, astroObject.SemiMajorAxis, astroObject.Inclination + 90, astroObject.ArgumentOfPeriapsis, astroObject.LongitudeOfAscendingNode, 90f);
            var a = astroObject.SemiMajorAxis;
            var b = a * Mathf.Sqrt(1 - astroObject.Eccentricity * astroObject.Eccentricity);

            _semiMajorAxis = periapsis.normalized * a;
            _semiMinorAxis = semiMinorDecending.normalized * b;
            _upAxisDir = Vector3.Cross(_semiMajorAxis, _semiMinorAxis).normalized;
            _fociDistance = Mathf.Sqrt(a * a - b * b);
        }
    }
}
