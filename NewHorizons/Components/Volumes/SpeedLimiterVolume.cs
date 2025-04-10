using System.Collections.Generic;
using System;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    public class SpeedLimiterVolume : BaseVolume
    {
        public float maxSpeed = 10f;
        public float stoppingDistance = 100f;
        public float maxEntryAngle = 60f;

        private OWRigidbody _parentBody;
        private List<TrackedBody> _trackedBodies = new List<TrackedBody>();
        private bool _playerJustExitedDream;

        public override void Awake()
        {
            _parentBody = GetComponentInParent<OWRigidbody>();
            base.Awake();
            GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
        }

        public void Start()
        {
            enabled = false;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
        }

        public void FixedUpdate()
        {
            foreach (var trackedBody in _trackedBodies)
            {
                bool slowed = false;
                Vector3 velocity = trackedBody.body.GetVelocity() - _parentBody.GetVelocity();
                float magnitude = velocity.magnitude;
                if (magnitude <= maxSpeed)
                {
                    slowed = true;
                }
                else
                {
                    bool needsSlowing = true;
                    float velocityReduction = trackedBody.deceleration * Time.deltaTime;
                    float requiredReduction = maxSpeed - magnitude;
                    if (requiredReduction > velocityReduction)
                    {
                        velocityReduction = requiredReduction;
                        slowed = true;
                    }
                    if (trackedBody.name == Detector.Name.Ship)
                    {
                        Autopilot component = Locator.GetShipTransform().GetComponent<Autopilot>();
                        if (component != null && component.IsFlyingToDestination())
                        {
                            needsSlowing = false;
                        }
                    }
                    if (needsSlowing)
                    {
                        Vector3 velocityChange = velocityReduction * velocity.normalized;
                        trackedBody.body.AddVelocityChange(velocityChange);
                        if (trackedBody.name == Detector.Name.Ship && PlayerState.IsInsideShip())
                        {
                            Locator.GetPlayerBody().AddVelocityChange(velocityChange);
                        }
                    }
                }
                if (slowed)
                {
                    if (trackedBody.name == Detector.Name.Ship)
                        GlobalMessenger.FireEvent("ShipExitSpeedLimiter");
                    _trackedBodies.Remove(trackedBody);
                    if (_trackedBodies.Count == 0)
                        enabled = false;
                }
            }
        }

        private void OnExitDreamWorld()
        {
            _playerJustExitedDream = true;
        }

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            DynamicForceDetector component = hitObj.GetComponent<DynamicForceDetector>();
            if (component == null || !component.CompareNameMask(Detector.Name.Player | Detector.Name.Probe | Detector.Name.Ship)) return;

            if (component.GetName() == Detector.Name.Player && (PlayerState.IsInsideShip() || _playerJustExitedDream))
            {
                _playerJustExitedDream = false;
            }
            else
            {
                OWRigidbody attachedOWRigidbody = component.GetAttachedOWRigidbody();
                Vector3 from = transform.position - attachedOWRigidbody.GetPosition();
                Vector3 to = attachedOWRigidbody.GetVelocity() - _parentBody.GetVelocity();
                float magnitude = to.magnitude;
                if (magnitude > maxSpeed && Vector3.Angle(from, to) < maxEntryAngle)
                {
                    float deceleration = (maxSpeed * maxSpeed - magnitude * magnitude) / (2f * stoppingDistance);
                    TrackedBody trackedBody = new TrackedBody(attachedOWRigidbody, component.GetName(), deceleration);
                    _trackedBodies.Add(trackedBody);
                    if (component.GetName() == Detector.Name.Ship)
                        GlobalMessenger.FireEvent("ShipEnterSpeedLimiter");
                    enabled = true;
                }
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {
            DynamicForceDetector component = hitObj.GetComponent<DynamicForceDetector>();
            if (component == null) return;

            OWRigidbody body = component.GetAttachedOWRigidbody();
            TrackedBody trackedBody = _trackedBodies.Find((TrackedBody i) => i.body == body);
            if (trackedBody != null)
            {
                if (trackedBody.name == Detector.Name.Ship)
                    GlobalMessenger.FireEvent("ShipExitSpeedLimiter");

                _trackedBodies.Remove(trackedBody);

                if (_trackedBodies.Count == 0)
                    enabled = false;
            }
        }

        [Serializable]
        protected class TrackedBody
        {
            public OWRigidbody body;

            public Detector.Name name;

            public float deceleration;

            public bool decelerated;

            public TrackedBody(OWRigidbody body, Detector.Name name, float deceleration)
            {
                this.body = body;
                this.name = name;
                this.deceleration = deceleration;
            }

            public override bool Equals(object obj)
            {
                if (obj is TrackedBody trackedBody)
                {
                    return trackedBody.body == body && trackedBody.name == name;
                }
                return base.Equals(obj);
            }

            public override int GetHashCode() => body.GetHashCode();
        }
    }
}
