#region

using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

#endregion

namespace NewHorizons.Components.Orbital
{
    public class NHOrbitLine : OrbitLine
    {
        public Vector3 SemiMajorAxis { get; set; }
        public Vector3 SemiMinorAxis { get; set; }

        private Vector3 _upAxis;
        private float _fociDistance;
        private Vector3[] _verts;

        private float semiMajorAxis;
        private float semiMinorAxis;

        public override void InitializeLineRenderer()
        {
            GetComponent<LineRenderer>().positionCount = _numVerts;
        }

        public override void OnValidate()
        {
            if (_numVerts < 0 || _numVerts > 4096) _numVerts = Mathf.Clamp(_numVerts, 0, 4096);
            if (GetComponent<LineRenderer>().positionCount != _numVerts) InitializeLineRenderer();
        }

        public override void Start()
        {
            base.Start();

            var a = SemiMajorAxis.magnitude;
            var b = SemiMinorAxis.magnitude;

            _upAxis = Vector3.Cross(SemiMajorAxis.normalized, SemiMinorAxis.normalized);

            _fociDistance = Mathf.Sqrt(a * a - b * b);
            if (float.IsNaN(_fociDistance)) _fociDistance = 0f;

            _verts = new Vector3[_numVerts];

            transform.localRotation = Quaternion.Euler(270, 90, 0);

            semiMajorAxis = SemiMajorAxis.magnitude;
            semiMinorAxis = SemiMinorAxis.magnitude;

            enabled = false;
        }

        public override void Update()
        {
            try
            {
                var primary = _astroObject?.GetPrimaryBody();

                // If it has nothing to orbit then why is this here
                if (primary == null)
                {
                    enabled = false;
                    return;
                }

                var origin = primary.transform.position + SemiMajorAxis.normalized * _fociDistance;

                var num = CalcProjectedAngleToCenter(origin, SemiMajorAxis, SemiMinorAxis,
                    _astroObject.transform.position);

                for (var i = 0; i < _numVerts; i++)
                {
                    var stepSize = 2f * Mathf.PI / (_numVerts - 1);
                    var f = num + stepSize * i;
                    _verts[i] = SemiMajorAxis * Mathf.Cos(f) + SemiMinorAxis * Mathf.Sin(f);
                }

                _lineRenderer.SetPositions(_verts);

                transform.position = origin;
                transform.rotation = Quaternion.Euler(0, 0, 0); //Quaternion.LookRotation(-SemiMajorAxis, _upAxis);

                var num2 = DistanceToEllipticalOrbitLine(origin, SemiMajorAxis, SemiMinorAxis, _upAxis,
                    Locator.GetActiveCamera().transform.position);
                var widthMultiplier = Mathf.Min(num2 * (_lineWidth / 1000f), _maxLineWidth);
                var num3 = _fade ? 1f - Mathf.Clamp01((num2 - _fadeStartDist) / (_fadeEndDist - _fadeStartDist)) : 1f;

                _lineRenderer.widthMultiplier = widthMultiplier;
                _lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, num3 * num3);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception in OrbitLine for [{_astroObject?.name}] : {ex.Message}, {ex.StackTrace}");
                enabled = false;
            }
        }

        private float CalcProjectedAngleToCenter(Vector3 foci, Vector3 semiMajorAxis, Vector3 semiMinorAxis,
            Vector3 point)
        {
            var lhs = point - foci;
            var vector = new Vector3(Vector3.Dot(lhs, semiMajorAxis.normalized), 0f,
                Vector3.Dot(lhs, semiMinorAxis.normalized));
            vector.x *= semiMinorAxis.magnitude / semiMajorAxis.magnitude;
            return Mathf.Atan2(vector.z, vector.x);
        }

        private float DistanceToEllipticalOrbitLine(Vector3 foci, Vector3 semiMajorAxis, Vector3 semiMinorAxis,
            Vector3 upAxis, Vector3 point)
        {
            var f = CalcProjectedAngleToCenter(foci, semiMajorAxis, semiMinorAxis, point);
            var b = foci + SemiMajorAxis * Mathf.Cos(f) + SemiMinorAxis * Mathf.Sin(f);
            return Vector3.Distance(point, b);
        }
    }
}