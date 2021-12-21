using NewHorizons.External;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.OrbitalPhysics;
using Logger = NewHorizons.Utility.Logger;
using System.Reflection;
using NewHorizons.Utility;

namespace NewHorizons.General
{
    static class InitialMotionBuilder
    {
        public static InitialMotion Make(GameObject body, AstroObject primaryBody, OWRigidbody OWRB, OrbitModule orbit)
        {


            /*
            ParameterizedInitialMotion initialMotion = body.AddComponent<ParameterizedInitialMotion>();
            initialMotion.SetPrimary(primaryBody);
            initialMotion.SetOrbitalParameters(
                orbit.Eccentricity,
                orbit.SemiMajorAxis,
                orbit.Inclination,
                orbit.LongitudeOfAscendingNode,
                orbit.ArgumentOfPeriapsis,
                orbit.TrueAnomaly);
            initialMotion.SetValue("_initAngularSpeed", orbit.SiderealPeriod == 0 ? 0.02f : 1.0f / orbit.SiderealPeriod);
            */

            InitialMotion initialMotion = body.AddComponent<InitialMotion>();
            initialMotion.SetPrimaryBody(primaryBody.GetAttachedOWRigidbody());
            initialMotion.SetValue("_orbitAngle", orbit.Inclination);
            initialMotion.SetValue("_isGlobalAxis", false);
            initialMotion.SetValue("_initAngularSpeed", orbit.SiderealPeriod == 0 ? 0.02f : 1.0f / orbit.SiderealPeriod);
            if(orbit.Eccentricity != 0)
            {
                // Calculate speed at apoapsis
                var eccSpeed = OrbitalHelper.Visviva(primaryBody.GetGravityVolume().GetFalloffType(),
                    primaryBody.GetGravityVolume().GetStandardGravitationalParameter(),
                    orbit.SemiMajorAxis * (1 + orbit.Eccentricity),
                    orbit.SemiMajorAxis,
                    orbit.Eccentricity
                    );
                var circularSpeed = OWPhysics.CalculateOrbitVelocity(primaryBody.GetAttachedOWRigidbody(), OWRB).magnitude;
                initialMotion.SetValue("_orbitImpulseScalar", eccSpeed / circularSpeed);
            }

            var rotationAxis = Quaternion.AngleAxis(orbit.AxialTilt + orbit.Inclination, Vector3.right) * Vector3.up;
            body.transform.rotation = Quaternion.FromToRotation(Vector3.up, rotationAxis);

            return initialMotion;
        }
    }
}
