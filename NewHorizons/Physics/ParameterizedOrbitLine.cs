using NewHorizons.Kepler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility
{
    public class ParameterizedOrbitLine : OrbitLine
    {
		protected override void InitializeLineRenderer()
		{
			base.GetComponent<LineRenderer>().positionCount = this._numVerts;
		}

		protected override void OnValidate()
		{
			if (this._numVerts < 0 || this._numVerts > 4096)
			{
				this._numVerts = Mathf.Clamp(this._numVerts, 0, 4096);
			}
			if (base.GetComponent<LineRenderer>().positionCount != this._numVerts)
			{
				this.InitializeLineRenderer();
			}
		}

		protected override void Start()
		{
			base.Start();
			this._verts = new Vector3[this._numVerts];
			base.enabled = false;
		}

		public void SetOrbitalParameters(float eccentricity, float semiMajorAxis, float inclination, float longitudeOfAscendingNode, float argumentOfPeriapsis, float trueAnomaly)
        {
			_eccentricity = eccentricity;
			_semiMajorAxis = semiMajorAxis;
			_inclination = inclination;
			_longitudeOfAscendingNode = longitudeOfAscendingNode;
			_argumentOfPeriapsis = argumentOfPeriapsis;
			_trueAnomaly = trueAnomaly;

			_initialMotion = (this._astroObject != null) ? this._astroObject.GetComponent<InitialMotion>() : null;
			_primary = (this._astroObject != null) ? this._astroObject.GetPrimaryBody() : null;

			if (_initialMotion && _primary)
            {
				var periapsisAngle = longitudeOfAscendingNode + argumentOfPeriapsis;
				var apoapsisAngle = periapsisAngle + 90f;

				_vSemiMinorAxis = OrbitalHelper.CartesianFromOrbitalElements(_eccentricity, _semiMajorAxis, _inclination, _longitudeOfAscendingNode, _argumentOfPeriapsis, periapsisAngle);
				_vSemiMajorAxis = OrbitalHelper.CartesianFromOrbitalElements(_eccentricity, _semiMajorAxis, _inclination, _longitudeOfAscendingNode, _argumentOfPeriapsis, apoapsisAngle);

				Vector3 rhs = this._astroObject.transform.position - _primary.transform.position;
				Vector3 initVelocity = _initialMotion.GetInitVelocity();
				Vector3 vector = Vector3.Cross(initVelocity, rhs);
				this._upAxisDir = vector.normalized;
				var semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - (eccentricity * eccentricity));

				_fociDistance = Mathf.Sqrt((semiMajorAxis * semiMajorAxis) - (semiMinorAxis * semiMinorAxis));

				base.enabled = true;
			}
			else
            {
				Logger.LogError($"Couldn't set values for KeplerOrbitLine. InitialMotion = {_initialMotion}, AstroObject = {_primary}");
            }
        }

		protected override void Update()
		{
			Vector3 vector = _primary.transform.position + _vSemiMajorAxis * _fociDistance;
			float num = CalcProjectedAngleToCenter(vector, _vSemiMajorAxis, _vSemiMinorAxis, _astroObject.transform.position);
			for (int i = 0; i < _numVerts; i++)
            {
				var angle = ((float)i / (float)(_numVerts - 1)) * 360f - Mathf.Rad2Deg * num;
				_verts[i] = OrbitalHelper.CartesianFromOrbitalElements(_eccentricity, _semiMajorAxis, _inclination, _longitudeOfAscendingNode, _argumentOfPeriapsis, angle);
            }
			_lineRenderer.SetPositions(_verts);

			// From EllipticalOrbitLine
			base.transform.position = vector;
			base.transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(-48f, Vector3.up) * _vSemiMajorAxis, -_upAxisDir);
			float num2 = DistanceToEllipticalOrbitLine(vector, _vSemiMajorAxis, _vSemiMinorAxis, _upAxisDir, Locator.GetActiveCamera().transform.position);
			float widthMultiplier = Mathf.Min(num2 * (_lineWidth / 1000f), _maxLineWidth);
			float num3 = _fade ? (1f - Mathf.Clamp01((num2 - _fadeStartDist) / (_fadeEndDist - _fadeStartDist))) : 1f;
			_lineRenderer.widthMultiplier = widthMultiplier;
			_lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, num3 * num3);
		}

		private float CalcProjectedAngleToCenter(Vector3 foci, Vector3 semiMajorAxis, Vector3 semiMinorAxis, Vector3 point)
		{
			Vector3 lhs = point - foci;
			Vector3 vector = new Vector3(Vector3.Dot(lhs, semiMajorAxis.normalized), 0f, Vector3.Dot(lhs, semiMinorAxis.normalized));
			vector.x *= semiMinorAxis.magnitude / semiMajorAxis.magnitude;
			return Mathf.Atan2(vector.z, vector.x);
		}

		private float DistanceToEllipticalOrbitLine(Vector3 foci, Vector3 semiMajorAxis, Vector3 semiMinorAxis, Vector3 upAxis, Vector3 point)
		{
			float f = this.CalcProjectedAngleToCenter(foci, semiMajorAxis, semiMinorAxis, point);
			Vector3 b = foci + this._vSemiMajorAxis * Mathf.Cos(f) + this._vSemiMinorAxis * Mathf.Sin(f);
			return Vector3.Distance(point, b);
		}

		private Vector3 _vSemiMajorAxis;
		private Vector3 _vSemiMinorAxis;
		private Vector3 _upAxisDir;
		private float _fociDistance;

		private float _eccentricity;
		private float _semiMajorAxis;
		private float _inclination;
		private float _longitudeOfAscendingNode;
		private float _argumentOfPeriapsis;
		private float _trueAnomaly;

		private Vector3[] _verts;
		private InitialMotion _initialMotion;
		private AstroObject _primary;
	}
}
