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
        }
	}
}
