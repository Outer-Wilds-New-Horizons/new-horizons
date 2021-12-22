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
    public class ParameterizedOrbitLine : OrbitLine
	{               
		/*
		protected override void InitializeLineRenderer()
		{
			base.GetComponent<LineRenderer>().positionCount = this._numVerts;
		}

		protected override void OnValidate()
		{
			if (_numVerts < 0 || _numVerts > 4096)
			{
				_numVerts = Mathf.Clamp(this._numVerts, 0, 4096);
			}
			if (base.GetComponent<LineRenderer>().positionCount != _numVerts)
			{
				InitializeLineRenderer();
			}
		}

		protected override void Start()
		{
			base.Start();
			_verts = new Vector3[_numVerts];
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

			_initialMotion = (_astroObject != null) ? _astroObject.GetComponent<InitialMotion>() : null;
			_primary = (_astroObject != null) ? _astroObject.GetPrimaryBody() : null;

			_falloffType = _primary.GetGravityVolume().GetFalloffType();
			if (_initialMotion && _primary)
            {
				_vSemiMajorAxis = OrbitalHelper.CartesianStateVectorsFromEccentricAnomaly(0f, _eccentricity, _semiMajorAxis, _inclination, _longitudeOfAscendingNode, _argumentOfPeriapsis, 0, _falloffType).Position;
				_vSemiMinorAxis = OrbitalHelper.CartesianStateVectorsFromEccentricAnomaly(0f, _eccentricity, _semiMajorAxis, _inclination, _longitudeOfAscendingNode, _argumentOfPeriapsis, 90, _falloffType).Position;
				_upAxisDir = Vector3.Cross(_vSemiMajorAxis, _vSemiMinorAxis);

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
			Vector3 vector = _primary.transform.position + _vSemiMajorAxis.normalized * _fociDistance;
			float nu = CalcProjectedAngleToCenter(vector, _vSemiMajorAxis, _vSemiMinorAxis, _astroObject.transform.position);

			for (int i = 0; i < _numVerts; i++)
            {
				var angle = Mathf.Rad2Deg * _trueAnomaly + (Mathf.Rad2Deg * nu) - ((float)i / (float)(_numVerts - 1)) * 360f;
				_verts[i] = OrbitalHelper.CartesianStateVectorsFromTrueAnomaly(0f, _eccentricity, _semiMajorAxis, _inclination, _longitudeOfAscendingNode, _argumentOfPeriapsis, angle, _falloffType).Position;
            }
			_lineRenderer.SetPositions(_verts);

			// From EllipticalOrbitLine
			base.transform.position = vector;
			base.transform.rotation = Quaternion.AngleAxis(0f, Vector3.up); 
			//base.transform.rotation = Quaternion.AngleAxis(_trueAnomaly + _longitudeOfAscendingNode + _argumentOfPeriapsis, Vector3.up);

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
		private OrbitalHelper.FalloffType _falloffType;
		*/
	}
}
