using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.OrbitalPhysics
{
    public class ParameterizedInitialMotion : InitialMotion
    {
        private new void Awake()
        {
            _rotationAxis = base.transform.TransformDirection(_rotationAxis);
            _satelliteBody = this.GetRequiredComponent<OWRigidbody>();
        }

        private new void Start()
        {
            _satelliteBody.UpdateCenterOfMass();
            _satelliteBody.AddVelocityChange(GetInitVelocity());
            _satelliteBody.AddAngularVelocityChange(GetInitAngularVelocity());
        }

        public new Vector2 GetOrbitEllipseSemiAxes()
        {
            var x = _semiMajorAxis;
            var y = _semiMajorAxis * Mathf.Sqrt(1 - _eccentricity * _eccentricity);

            return new Vector2(x, y);
        }

        public new Vector3 GetInitAngularVelocity()
        {
            return _rotationAxis.normalized * _initAngularSpeed;
        }

        public new OWRigidbody GetPrimaryBody()
        {
            return _primary.GetAttachedOWRigidbody();
        }

        public new void SetPrimaryBody(OWRigidbody primaryBody)
        {
            _primary = primaryBody.gameObject.GetComponent<AstroObject>();
        }

        public new Vector3 GetInitVelocity()
        {
            if(_initVelocityDirty)
            {
                var state = OrbitalHelper.CartesianStateVectorsFromTrueAnomaly(
                    _primary.GetGravityVolume().GetStandardGravitationalParameter(),
                    _eccentricity,
                    _semiMajorAxis,
                    _inclination,
                    _longitudeOfAscendingNode,
                    _argumentOfPeriapsis,
                    _trueAnomaly,
                    _primary.GetGravityVolume().GetFalloffType()
                    );
                _initVelocity = state.Velocity;

                if(_primary?.GetComponent<InitialMotion>() != null)
                {
                    var primaryVel = _primary.GetComponent<InitialMotion>().GetInitVelocity();
                    Logger.Log($"{primaryVel}");
                    _initVelocity += primaryVel;
                }

                _initVelocityDirty = false;
            }
            return _initVelocity;
        }

        public void SetOrbitalParameters(float eccentricity, float semiMajorAxis, float inclination, float longitudeOfAscendingNode, float argumentOfPeriapsis, float trueAnomaly)
        {
            _eccentricity = eccentricity;
            _semiMajorAxis = semiMajorAxis;
            _inclination = inclination;
            _longitudeOfAscendingNode = longitudeOfAscendingNode;
            _argumentOfPeriapsis = argumentOfPeriapsis;
            _trueAnomaly = trueAnomaly;
        }

        public void SetPrimary(AstroObject primary)
        {
            _primary = primary;
        }

        private float _eccentricity;
        private float _semiMajorAxis;
        private float _inclination;
        private float _longitudeOfAscendingNode;
        private float _argumentOfPeriapsis;
        private float _trueAnomaly;

        private AstroObject _primary;

        private bool _initVelocityDirty = true;
        private Vector3 _initVelocity;
        private Vector3 _rotationAxis = Vector3.up;

        private float _initAngularSpeed;

        private OWRigidbody _satelliteBody;
    }
}
