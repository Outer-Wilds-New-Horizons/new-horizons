using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.OrbitalPhysics
{
    public class TrackingOrbitLine : OrbitLine
    {
		private Vector3[] _vertices;
		private float _timer;
		private bool _firstTimeEnabled = true;
		
		public float TrailTime = 120f;


		protected override void InitializeLineRenderer()
		{
			_lineRenderer.positionCount = this._numVerts;
		}

		protected override void OnValidate()
		{
			if (_numVerts < 0 || _numVerts > 4096)
			{
				_numVerts = Mathf.Clamp(this._numVerts, 0, 4096);
			}
			if (_lineRenderer.positionCount != _numVerts)
			{
				InitializeLineRenderer();
			}
		}

		protected override void Start()
		{
			base.Start();
			_vertices = new Vector3[_numVerts];

			base.enabled = true;
			_lineRenderer.enabled = false;
		}

		protected override void OnEnterMapView()
		{
			if (_firstTimeEnabled) 
			{
				ResetLineVertices();
				_firstTimeEnabled = false;
			}
			_lineRenderer.enabled = true;
		}

		protected override void OnExitMapView()
		{
			_lineRenderer.enabled = false;
		}

		protected override void Update()
		{
			var primary = _astroObject.GetPrimaryBody();
			Vector3 origin = primary == null ? Locator.GetRootTransform().position : primary.transform.position;

			_timer += Time.deltaTime;
			var updateTime = (TrailTime / (float)_numVerts);

			if(_timer > updateTime)
            {
				for (int i = _numVerts - 1; i > 0; i--)
				{
					var v = _vertices[i - 1];
					_vertices[i] = new Vector3(v.x, v.y, v.z);
				}
				_vertices[0] = transform.parent.position - origin;
				_lineRenderer.SetPositions(_vertices);
				_timer = 0;
			}

			base.transform.position = origin;
			base.transform.rotation = Quaternion.AngleAxis(0f, Vector3.up);

			float num2 = DistanceToTrackingOrbitLine(Locator.GetActiveCamera().transform.position);
			float widthMultiplier = Mathf.Min(num2 * (_lineWidth / 1000f), _maxLineWidth);
			float num3 = _fade ? (1f - Mathf.Clamp01((num2 - _fadeStartDist) / (_fadeEndDist - _fadeStartDist))) : 1f;
			_lineRenderer.widthMultiplier = widthMultiplier;
			_lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, num3 * num3);
		}

		private float DistanceToTrackingOrbitLine(Vector3 point)
		{
			// Check against 3 points on the line
			var primary = _astroObject.GetPrimaryBody();
			point -= primary.transform.position;
			var dist1 = Vector3.Distance(point, _vertices[0]);
			var dist2 = Vector3.Distance(point, _vertices[(int)(_numVerts / 2)]);
			var dist3 = Vector3.Distance(point, _vertices[_numVerts - 1]);
			
			return Mathf.Min(new float[] { dist1, dist2, dist3 });
		}

		public void ResetLineVertices()
        {
			var primary = _astroObject.GetPrimaryBody();
			Vector3 origin = primary == null ? Locator.GetRootTransform().position : primary.transform.position;

			base.transform.position = origin;
			base.transform.rotation = Quaternion.AngleAxis(0f, Vector3.up);

			for (int i = _numVerts - 1; i > 0; i--)
			{
				_vertices[i] = transform.parent.position - origin;
			}
			_lineRenderer.SetPositions(_vertices);
		}
	}
}
