using NewHorizons.External;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using System.Reflection;
using NewHorizons.Utility;
using NewHorizons.Components.Orbital;

namespace NewHorizons.Builder.Orbital
{
    static class InitialMotionBuilder
    {
        public static InitialMotion Make(GameObject body, AstroObject primaryBody, AstroObject secondaryBody, OWRigidbody OWRB, OrbitModule orbit)
        {
            InitialMotion initialMotion = body.AddComponent<InitialMotion>();
            return Update(initialMotion, body, primaryBody, secondaryBody, OWRB, orbit);
        }

        public static float SiderealPeriodToAngularSpeed(float siderealPeriod)
        {
            return siderealPeriod == 0 ? 0f : 2f * Mathf.PI / (siderealPeriod * 60f);
        }

        public static InitialMotion Update(InitialMotion initialMotion, GameObject body, AstroObject primaryBody, AstroObject secondaryBody, OWRigidbody OWRB, OrbitModule orbit)
        {
            // Rotation
            initialMotion._initAngularSpeed = SiderealPeriodToAngularSpeed(orbit.SiderealPeriod);

            var rotationAxis = Quaternion.AngleAxis(orbit.AxialTilt + 90f, Vector3.right) * Vector3.up;
            body.transform.rotation = Quaternion.FromToRotation(Vector3.up, rotationAxis);

            initialMotion._orbitImpulseScalar = 0f;
            if (!orbit.IsStatic && primaryBody != null)
            {                
                initialMotion.SetPrimaryBody(primaryBody.GetAttachedOWRigidbody());
                var gv = primaryBody.GetGravityVolume();
                if(gv != null)
                {
                    var primaryGravity = new Gravity(primaryBody.GetGravityVolume());
                    var secondaryGravity = new Gravity(secondaryBody.GetGravityVolume());
                    var velocity = orbit.GetOrbitalParameters(primaryGravity, secondaryGravity).InitialVelocity;

                    initialMotion._initLinearDirection = velocity.normalized;
                    initialMotion._initLinearSpeed = velocity.magnitude;
                }
            }

            return initialMotion;
        }
    }
}
