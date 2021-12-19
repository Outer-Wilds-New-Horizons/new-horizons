using NewHorizons.External;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class InitialMotionBuilder
    {
        public static InitialMotion Make(GameObject body, AstroObject primaryBody, OWRigidbody OWRB, Vector3 positionVector, OrbitModule orbit)
        {
            InitialMotion IM = body.AddComponent<InitialMotion>();
            IM.SetPrimaryBody(primaryBody.GetAttachedOWRigidbody());
            IM.SetValue("_orbitAngle", orbit.Inclination);
            IM.SetValue("_isGlobalAxis", false);
            IM.SetValue("_initAngularSpeed", orbit.SiderealPeriod == 0 ? 0.02f : 1.0f / orbit.SiderealPeriod);

            // Initial velocity
            var distance = positionVector - primaryBody.transform.position;
            var speed = Kepler.OrbitalHelper.VisViva(primaryBody.GetGravityVolume().GetStandardGravitationalParameter(), distance, orbit.SemiMajorAxis);
            var direction = Kepler.OrbitalHelper.EllipseTangent(orbit.Eccentricity, orbit.SemiMajorAxis, orbit.Inclination, orbit.LongitudeOfAscendingNode, orbit.ArgumentOfPeriapsis, orbit.TrueAnomaly);
            var velocity = speed * direction;

            // Cancel out what the game does
            if(orbit.Eccentricity != 0)
            {
                var oldVelocity = Kepler.OrbitalHelper.CalculateOrbitVelocity(primaryBody.GetAttachedOWRigidbody(), distance, orbit.Inclination);
                if (!float.IsNaN(oldVelocity.magnitude)) velocity -= oldVelocity;
                else Logger.LogError($"The original orbital velocity for {body.name} was calculated to be NaN?");

                var trueSpeed = velocity.magnitude;
                var trueDirection = velocity.normalized;

                Logger.Log($"Setting initial motion {velocity.magnitude} in direction {velocity.normalized}");
                if (!float.IsNaN(trueSpeed) && trueDirection != Vector3.zero)
                {
                    IM.SetValue("_initLinearDirection", velocity.normalized);
                    IM.SetValue("_initLinearSpeed", velocity.magnitude);
                }
                else Logger.LogError($"Could not set velocity ({speed}, {direction}) -> ({trueSpeed}, {trueDirection}) for {body.name}");
            }

            var rotationAxis = Quaternion.AngleAxis(orbit.AxialTilt + orbit.Inclination, Vector3.right) * Vector3.up;
            body.transform.rotation = Quaternion.FromToRotation(Vector3.up, rotationAxis);

            return IM;
        }
    }
}
