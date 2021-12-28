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

namespace NewHorizons.Builder.Orbital
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
            return Update(initialMotion, body, primaryBody, OWRB, orbit);
        }

        public static InitialMotion Update(InitialMotion initialMotion, GameObject body, AstroObject primaryBody, OWRigidbody OWRB, OrbitModule orbit)
        {
            if (!orbit.IsStatic)
            {
                initialMotion._orbitImpulseScalar = 0f;
                
                if(primaryBody != null)
                {
                    initialMotion.SetPrimaryBody(primaryBody.GetAttachedOWRigidbody());
                    var gv = primaryBody.GetGravityVolume();
                    if(gv != null)
                    {
                        var velocity = OrbitalHelper.GetVelocity(new OrbitalHelper.Gravity(primaryBody.GetGravityVolume()), orbit);
                        initialMotion._initLinearDirection = velocity.normalized;
                        initialMotion._initLinearSpeed = velocity.magnitude;
                    }
                }
            }

            // Rotation
            //initialMotion.SetValue("_initAngularSpeed", orbit.SiderealPeriod == 0 ? 0f : 1.0f / orbit.SiderealPeriod);
            initialMotion.SetValue("_initAngularSpeed", 0);
            var rotationAxis = Quaternion.AngleAxis(orbit.AxialTilt + orbit.Inclination, Vector3.right) * Vector3.up;
            body.transform.rotation = Quaternion.FromToRotation(Vector3.up, rotationAxis);

            return initialMotion;
        }
    }
}
