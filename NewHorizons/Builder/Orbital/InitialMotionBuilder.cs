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
            InitialMotion initialMotion = body.AddComponent<InitialMotion>();
            return Update(initialMotion, body, primaryBody, OWRB, orbit);
        }

        public static InitialMotion Update(InitialMotion initialMotion, GameObject body, AstroObject primaryBody, OWRigidbody OWRB, OrbitModule orbit)
        {
            // Rotation
            initialMotion.SetValue("_initAngularSpeed", orbit.SiderealPeriod == 0 ? 0f : 2f * Mathf.PI / (orbit.SiderealPeriod * 60f));

            var rotationAxis = Quaternion.AngleAxis(orbit.AxialTilt + 90f, Vector3.right) * Vector3.up;
            body.transform.rotation = Quaternion.FromToRotation(Vector3.up, rotationAxis);

            initialMotion._orbitImpulseScalar = 0f;
            if (!orbit.IsStatic)
            {                
                if(primaryBody != null)
                {
                    initialMotion.SetPrimaryBody(primaryBody.GetAttachedOWRigidbody());
                    var gv = primaryBody.GetGravityVolume();
                    if(gv != null)
                    {
                        var velocity = OrbitalHelper.GetCartesian(new OrbitalHelper.Gravity(primaryBody.GetGravityVolume()), orbit).Item2;

                        // For some stupid reason the InitialMotion awake method transforms the perfectly fine direction vector you give it so we preemptively do the inverse so it all cancels out
                        initialMotion._initLinearDirection = body.transform.InverseTransformDirection(velocity.normalized);
                        initialMotion._initLinearSpeed = velocity.magnitude;
                    }
                }
            }

            return initialMotion;
        }
    }
}
