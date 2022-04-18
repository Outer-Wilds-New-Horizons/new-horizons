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
            initialMotion._primaryBody = primaryBody?.GetAttachedOWRigidbody();

            var rotationAxis = Quaternion.AngleAxis(orbit.AxialTilt + 90f, Vector3.right) * Vector3.up;
            initialMotion._rotationAxis = rotationAxis;

            if (!orbit.IsStatic && primaryBody != null)
            {
                var focalPoint = primaryBody.GetComponent<BinaryFocalPoint>();
                if (focalPoint)
                {
                    // Focal stuff
                    var name = secondaryBody.GetCustomName();
                    if (name == focalPoint.PrimaryName || name == focalPoint.SecondaryName)
                    {
                        // The one we're currently looking at is always null
                        var otherBody = focalPoint.Primary ?? focalPoint.Secondary;
                        if (otherBody != null)
                        {
                            // We set the positions and velocities of both right now
                            if (name == focalPoint.PrimaryName)
                            {
                                SetBinaryInitialMotion(primaryBody, secondaryBody as NHAstroObject, otherBody as NHAstroObject);
                            }
                            else
                            {
                                SetBinaryInitialMotion(primaryBody, otherBody as NHAstroObject, secondaryBody as NHAstroObject);
                            }
                        }
                    }
                    else
                    {
                        // It's a circumbinary moon/planet
                        var fakePrimaryBody = focalPoint.FakeMassBody.GetComponent<AstroObject>();
                        SetMotionFromPrimary(fakePrimaryBody, secondaryBody as NHAstroObject, initialMotion);
                    }
                }
                else if (primaryBody.GetGravityVolume())
                {
                    SetMotionFromPrimary(primaryBody, secondaryBody as NHAstroObject, initialMotion);
                }
                else
                {
                    Logger.Log($"No primary gravity or focal point for {primaryBody}");
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

        private static void SetMotionFromPrimary(AstroObject primaryBody, NHAstroObject secondaryBody, InitialMotion initialMotion)
        {
            initialMotion.SetPrimaryBody(primaryBody.GetAttachedOWRigidbody());

            var primaryGravity = new Gravity(primaryBody.GetGravityVolume());
            var secondaryGravity = new Gravity(secondaryBody.GetGravityVolume());
            var velocity = secondaryBody.GetOrbitalParameters(primaryGravity, secondaryGravity).InitialVelocity;

            var parentVelocity = primaryBody?.GetComponent<InitialMotion>()?.GetInitVelocity() ?? Vector3.zero;
            initialMotion._cachedInitVelocity = velocity + parentVelocity;
            initialMotion._isInitVelocityDirty = false;
        }

        private static void SetBinaryInitialMotion(AstroObject baryCenter, NHAstroObject primaryBody, NHAstroObject secondaryBody)
        {
            Logger.Log($"Setting binary initial motion [{primaryBody.name}] [{secondaryBody.name}]");

            var primaryGravity = new Gravity(primaryBody._gravityVolume);
            var secondaryGravity = new Gravity(secondaryBody._gravityVolume);

            // Might make binaries with binaries with binaries work 
            if (primaryBody.GetGravityVolume() == null)
            {
                primaryGravity = new Gravity(primaryBody.GetComponent<BinaryFocalPoint>()?.FakeMassBody?.GetComponent<AstroObject>()?.GetGravityVolume());
            }
            if (secondaryBody.GetGravityVolume() == null)
            {
                secondaryGravity = new Gravity(secondaryBody.GetComponent<BinaryFocalPoint>()?.FakeMassBody?.GetComponent<AstroObject>()?.GetGravityVolume());
            }

            // Update the positions
            var distance = secondaryBody.GetOrbitalParameters(primaryGravity, secondaryGravity).InitialPosition;
            var m1 = primaryGravity.Mass;
            var m2 = secondaryGravity.Mass;

            var r1 = distance.magnitude * m2 / (m1 + m2);
            var r2 = distance.magnitude * m1 / (m1 + m2);

            primaryBody.transform.position = baryCenter.transform.position + r1 * distance.normalized;
            secondaryBody.transform.position = baryCenter.transform.position - r2 * distance.normalized;

            // Update the velocities
            var reducedMass = 1f / ((1f / m1) + (1f / m2));
            var reducedMassGravity = new Gravity(reducedMass, primaryGravity.Power);
            var baryCenterVelocity = baryCenter?.GetComponent<InitialMotion>()?.GetInitVelocity() ?? Vector3.zero;

            var primaryOrbit = OrbitalParameters.FromTrueAnomaly(
                reducedMassGravity,
                primaryGravity,
                primaryBody.Eccentricity,
                primaryBody.SemiMajorAxis,
                primaryBody.Inclination,
                primaryBody.ArgumentOfPeriapsis,
                primaryBody.LongitudeOfAscendingNode,
                0f
            );

            var primaryVelocity = primaryOrbit.InitialVelocity;
            var primaryInitialMotion = primaryBody.GetComponent<InitialMotion>();
            primaryInitialMotion._cachedInitVelocity = primaryVelocity + baryCenterVelocity;
            primaryInitialMotion._isInitVelocityDirty = false;

            var secondaryOrbit = OrbitalParameters.FromTrueAnomaly(
                reducedMassGravity,
                secondaryGravity,
                primaryBody.Eccentricity,
                primaryBody.SemiMajorAxis,
                primaryBody.Inclination,
                primaryBody.ArgumentOfPeriapsis,
                primaryBody.LongitudeOfAscendingNode,
                180f
            );

            var secondaryVelocity = secondaryOrbit.InitialVelocity;
            var secondaryInitialMotion = secondaryBody.GetComponent<InitialMotion>();
            secondaryInitialMotion._cachedInitVelocity = secondaryVelocity + baryCenterVelocity;
            secondaryInitialMotion._isInitVelocityDirty = false;

            Logger.Log($"Binary Initial Motion: {m1}, {m2}, {r1}, {r2}, {primaryVelocity}, {secondaryVelocity}");
        }
    }
}
