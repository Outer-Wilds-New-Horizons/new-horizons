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

            var periapsis = OrbitalHelper.GetPositionFromEccentricAnomaly(astroObject.Eccentricity, astroObject.SemiMajorAxis, astroObject.Inclination, astroObject.ArgumentOfPeriapsis, astroObject.LongitudeOfAscendingNode, 0f);
            var semiMinorDecending = OrbitalHelper.GetPositionFromEccentricAnomaly(astroObject.Eccentricity, astroObject.SemiMajorAxis, astroObject.Inclination, astroObject.ArgumentOfPeriapsis, astroObject.LongitudeOfAscendingNode, 90f);
            var a = astroObject.SemiMajorAxis;
            var b = a * Mathf.Sqrt(1 - astroObject.Eccentricity * astroObject.Eccentricity);

            _semiMajorAxis = periapsis.normalized * a;
            _semiMinorAxis = semiMinorDecending.normalized * b;
            _upAxisDir = Vector3.Cross(_semiMajorAxis, _semiMinorAxis).normalized;
            _fociDistance = Mathf.Sqrt(a * a - b * b);
        }

		// Literally the stock EllipticOrbitLine but for now I don't want to use the patches from CommonResources
		public override void Update()
		{
			AstroObject primaryAO = _astroObject?.GetPrimaryBody();
			if (primaryAO == null)
			{
				base.enabled = false;
				return;
			}
			Vector3 vector = primaryAO.transform.position + this._semiMajorAxis.normalized * this._fociDistance;
			float num = this.CalcProjectedAngleToCenter(vector, this._semiMajorAxis, this._semiMinorAxis, this._astroObject.transform.position);
			for (int i = 0; i < this._numVerts; i++)
			{
				float f = (float)i / (float)(this._numVerts - 1) * 3.1415927f * 2f - (num + 3.1415927f);
				this._verts[i] = this._semiMajorAxis * Mathf.Cos(f) + this._semiMinorAxis * Mathf.Sin(f);
			}
			this._lineRenderer.SetPositions(this._verts);
			base.transform.position = vector;
			base.transform.rotation = Quaternion.LookRotation(this._semiMinorAxis, this._upAxisDir);
			float num2 = this.DistanceToEllipticalOrbitLine(vector, this._semiMajorAxis, this._semiMinorAxis, this._upAxisDir, Locator.GetActiveCamera().transform.position);
			float widthMultiplier = Mathf.Min(num2 * (this._lineWidth / 1000f), this._maxLineWidth);
			float num3 = this._fade ? (1f - Mathf.Clamp01((num2 - this._fadeStartDist) / (this._fadeEndDist - this._fadeStartDist))) : 1f;
			this._lineRenderer.widthMultiplier = widthMultiplier;
			this._lineRenderer.startColor = new Color(this._color.r, this._color.g, this._color.b, num3 * num3);
		}
	}
}
