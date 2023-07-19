using System;
using UnityEngine;
using NewHorizons.Utility.OWML;

namespace NewHorizons.Components.Orbital
{
    public class NHOrbitLine : OrbitLine
    {
        private Vector3 _semiMajorAxis;
        private Vector3 _semiMinorAxis;

        private Vector3 _upAxis;
        private float _fociDistance;
        private Vector3[] _verts;

        public override void InitializeLineRenderer()
        {
            GetComponent<LineRenderer>().positionCount = this._numVerts;
        }

        public override void OnValidate()
        {
            if (_numVerts < 0 || _numVerts > 4096)
            {
                _numVerts = Mathf.Clamp(_numVerts, 0, 4096);
            }
            if (GetComponent<LineRenderer>().positionCount != this._numVerts)
            {
                InitializeLineRenderer();
            }
        }

        public override void Start()
        {
            base.Start();

            var a = _semiMajorAxis.magnitude;
            var b = _semiMinorAxis.magnitude;

            _upAxis = Vector3.Cross(_semiMajorAxis.normalized, _semiMinorAxis.normalized);

            _fociDistance = Mathf.Sqrt(a * a - b * b);
            if (float.IsNaN(_fociDistance)) _fociDistance = 0f;

            _verts = new Vector3[this._numVerts];

            transform.localRotation = Quaternion.Euler(270, 90, 0);

            enabled = false;
        }

        public override void Update()
        {
            try
            {
                AstroObject primary = _astroObject?.GetPrimaryBody();

                // If it has nothing to orbit then why is this here
                if (primary == null || !primary.gameObject.activeSelf)
                {
                    enabled = false;
                    return;
                }

                Vector3 origin = primary.transform.position + _semiMajorAxis.normalized * _fociDistance;

                float num = CalcProjectedAngleToCenter(origin, _semiMajorAxis, _semiMinorAxis, _astroObject.transform.position);

                for (int i = 0; i < _numVerts; i++)
                {
                    var stepSize = 2f * Mathf.PI / (float)(_numVerts - 1);
                    float f = num + stepSize * i;
                    _verts[i] = _semiMajorAxis * Mathf.Cos(f) + _semiMinorAxis * Mathf.Sin(f);
                }
                _lineRenderer.SetPositions(_verts);

                transform.position = origin;
                transform.rotation = Quaternion.Euler(0, 0, 0); //Quaternion.LookRotation(-SemiMajorAxis, _upAxis);

                float num2 = DistanceToEllipticalOrbitLine(origin, _semiMajorAxis, _semiMinorAxis, _upAxis, Locator.GetActiveCamera().transform.position);
                float widthMultiplier = Mathf.Min(num2 * (_lineWidth / 1000f), _maxLineWidth);
                float num3 = _fade ? (1f - Mathf.Clamp01((num2 - _fadeStartDist) / (_fadeEndDist - _fadeStartDist))) : 1f;

                _lineRenderer.widthMultiplier = widthMultiplier;
                _lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, num3 * num3);
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Exception in OrbitLine for [{_astroObject?.name}]:\n{ex}");
                enabled = false;
            }
        }

        public void SetFromParameters(IOrbitalParameters parameters)
        {
            var a = parameters.semiMajorAxis;
            var e = parameters.eccentricity;
            var b = a * Mathf.Sqrt(1f - (e * e));
            var l = parameters.longitudeOfAscendingNode;
            var p = parameters.argumentOfPeriapsis;
            var i = parameters.inclination;

            _semiMajorAxis = a * OrbitalParameters.Rotate(Vector3.left, l, i, p);
            _semiMinorAxis = b * OrbitalParameters.Rotate(Vector3.forward, l, i, p);
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
            float f = CalcProjectedAngleToCenter(foci, semiMajorAxis, semiMinorAxis, point);
            Vector3 b = foci + _semiMajorAxis * Mathf.Cos(f) + _semiMinorAxis * Mathf.Sin(f);
            return Vector3.Distance(point, b);
        }
    }
}
