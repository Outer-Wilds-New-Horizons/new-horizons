using System.Collections.Generic;
using UnityEngine;
namespace NewHorizons.Components
{
    public class NHFluidVolume : RadialFluidVolume
    {
        public float _surfaceCurrentSpeed = 10f;
        public float _equatorRepelAngle = 20f;
        public float _equatorRepelSpeed = 1f;
        public float _barrierRepelSpeed = 50f;

        public float _barrierUpperRadius => _radius * 0.92f;
        public float _barrierLowerRadius => _radius * 0.88f;

        public WaveHeightCalculator _waveHeightCalculator;

        protected List<SplashData> _splashData = new List<SplashData>(16);

        public override void Awake()
        {
            _waveHeightCalculator = gameObject.GetRequiredComponent<WaveHeightCalculator>();
            base.Awake();
        }

        public override float GetDepthAtPosition(Vector3 worldPosition)
        {
            Vector3 vector = transform.InverseTransformPoint(worldPosition);
            float dist = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z + vector.y * vector.y);
            return dist - _radius;
        }

        public override bool CheckTriggerProbeDragIncrease(FluidDetector probeDetector) => false;

        public override Vector3 GetPointFluidVelocity(
          Vector3 worldPosition,
          FluidDetector detector)
        {
            return _attachedBody.GetPointVelocity(worldPosition) + GetOceanCurrentVelocity(transform.InverseTransformPoint(worldPosition), detector);
        }

        public Vector3 GetPointBarrierVelocity(Vector3 worldPosition, FluidDetector detector)
        {
            Vector3 vector = transform.InverseTransformPoint(worldPosition);
            float dist = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z + vector.y * vector.y);
            float barrierDepthFraction = Mathf.InverseLerp(_barrierUpperRadius, _barrierLowerRadius, dist);
            HandleBarrierRumble(detector, dist, barrierDepthFraction);
            return vector.normalized * 50 * barrierDepthFraction;
        }

        public bool IsPointInBarrierZone(Vector3 worldPosition)
        {
            Vector3 vector = transform.InverseTransformPoint(worldPosition);
            float dist = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z + vector.y * vector.y);
            return dist > _barrierLowerRadius && dist < _barrierUpperRadius;
        }

        public override float GetFractionSubmerged(FluidDetector detector)
        {
            Vector3 vector = transform.InverseTransformPoint(detector.transform.position);
            float dist = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z + vector.y * vector.y);
            return detector.GetBuoyancyData().CalculateSubmergedFraction(dist, detector.GetBuoyancyData().checkAgainstWaves ? _waveHeightCalculator.GetOceanRadius(detector.transform.position) : _radius);
        }

        public override void RegisterSplashTransform(Transform splashTransform) => _splashData.Add(new SplashData(splashTransform, transform.position));

        public void FixedUpdate()
        {
            for (int index = _splashData.Count - 1; index >= 0; --index)
            {
                if (_splashData[index].splashTransform == null)
                {
                    _splashData.RemoveAt(index);
                }
                else
                {
                    Vector3 position = _splashData[index].splashTransform.position;
                    Vector3 oceanCurrentVelocity = GetOceanCurrentVelocity(position - transform.position, null);
                    Vector3 toDirection = (position + oceanCurrentVelocity * Time.fixedDeltaTime - transform.position).normalized * _splashData[index].altitude;
                    _splashData[index].splashTransform.position = transform.position + toDirection;
                    _splashData[index].splashTransform.rotation = Quaternion.FromToRotation(_splashData[index].splashTransform.up, toDirection) * _splashData[index].splashTransform.rotation;
                }
            }
        }

        protected Vector3 GetOceanCurrentVelocity(Vector3 displacement, FluidDetector detector)
        {
            Vector3 zero = Vector3.zero;
            float magnitude = Mathf.Sqrt(displacement.x * displacement.x + displacement.z * displacement.z + displacement.y * displacement.y);
            if (magnitude > _barrierLowerRadius)
            {
                float barrierDepthFraction = Mathf.InverseLerp(_barrierUpperRadius, _barrierLowerRadius, magnitude);
                zero += displacement.normalized * _barrierRepelSpeed * barrierDepthFraction;
                HandleBarrierRumble(detector, magnitude, barrierDepthFraction);
                float angle = Vector3.Angle(displacement, transform.up);
                float direction = angle < 90f ? 1f : -1f;
                Vector3 vector = direction * Vector3.Cross(displacement, transform.up).normalized;
                float correctedAngle = angle < 90f ? angle : 180f - angle;
                if (correctedAngle > _equatorRepelAngle)
                {
                    zero += (_surfaceCurrentSpeed * Mathf.Sin(Mathf.PI / 180f * correctedAngle) + barrierDepthFraction * _barrierRepelSpeed) * vector + direction * 90 * _equatorRepelSpeed * transform.up;
                }
            }
            return zero;
        }

        protected void HandleBarrierRumble(
          FluidDetector detector,
          float altitude,
          float barrierDepthFraction)
        {
            if (detector == null) return;

            if (detector.AffectsRumble()) RumbleManager.AddFluidRumble(_fluidType, barrierDepthFraction);

            if (altitude >= _barrierUpperRadius) return;

            if (detector is ProbeFluidDetector probeFluidDetector) probeFluidDetector.SetOceanBarrierDrag();
        }

        protected class SplashData
        {
            public Transform splashTransform;
            public float altitude;

            public SplashData(Transform splashTransform, Vector3 fluidPosition)
            {
                this.splashTransform = splashTransform;
                this.altitude = Vector3.Distance(fluidPosition, splashTransform.position);
            }
        }
    }
}
