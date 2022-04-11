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
            body.SetActive(false);
            InitialMotion initialMotion = body.AddComponent<InitialMotion>();

            // This bit makes the initial motion not try to calculate the orbit velocity itself for reasons
            initialMotion._orbitImpulseScalar = 0f;

            // Rotation
            initialMotion._initAngularSpeed = orbit.SiderealPeriod == 0 ? 0f : 2f * Mathf.PI / (orbit.SiderealPeriod * 60f);
            //initialMotion._primaryBody = primaryBody?.GetAttachedOWRigidbody();

            var rotationAxis = Quaternion.AngleAxis(orbit.AxialTilt + 90f, Vector3.right) * Vector3.up;
            initialMotion._rotationAxis = rotationAxis;

            if (!orbit.IsStatic && primaryBody != null)
            {                
                initialMotion.SetPrimaryBody(primaryBody.GetAttachedOWRigidbody());
                var gv = primaryBody.GetGravityVolume();
                if(gv != null)
                {
                    var primaryGravity = new Gravity(primaryBody.GetGravityVolume());
                    var secondaryGravity = new Gravity(secondaryBody.GetGravityVolume());
                    var velocity = orbit.GetOrbitalParameters(primaryGravity, secondaryGravity).InitialVelocity;

                    initialMotion._cachedInitVelocity = velocity + primaryBody?.GetComponent<InitialMotion>()?.GetInitVelocity() ?? Vector3.zero;
                    initialMotion._isInitVelocityDirty = false;
                }
            }
            else
            {
                initialMotion._initLinearDirection = Vector3.forward;
                initialMotion._initLinearSpeed = 0f;
            }


            body.SetActive(true);

            return initialMotion;
        }
    }
}
