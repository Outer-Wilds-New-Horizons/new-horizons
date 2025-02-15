using NewHorizons.Components.Orbital;
using NewHorizons.External.Modules;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Builder.Orbital
{
    public static class InitialMotionBuilder
    {
        public static InitialMotion Make(GameObject body, AstroObject primaryBody, AstroObject secondaryBody, OWRigidbody OWRB, OrbitModule orbit)
        {
            // Doing it like this so the planet orbit updater can just use an existing initial motion with the other method
            InitialMotion initialMotion = body.AddComponent<InitialMotion>();
            return SetInitialMotionFromConfig(initialMotion, primaryBody, secondaryBody, orbit);
        }

        public static InitialMotion SetInitialMotionFromConfig(InitialMotion initialMotion, AstroObject primaryBody, AstroObject secondaryBody, OrbitModule orbit, bool isCustom = true)
        {
            // This bit makes the initial motion not try to calculate the orbit velocity itself for reasons
            initialMotion._orbitImpulseScalar = 0f;

            // Rotation
            initialMotion._initAngularSpeed = orbit.siderealPeriod == 0 ? 0f : 2f * Mathf.PI / (orbit.siderealPeriod * 60f);
            var rotationAxis = Quaternion.AngleAxis(orbit.axialTilt, Vector3.right) * Vector3.up;

            // For stock planets with unsuspended rigidbody children this is broken
            // For planets with rafts this is broken
            if (AstroObjectLocator.GetChildren(secondaryBody).Length == 0)
            {
                secondaryBody.transform.rotation = Quaternion.FromToRotation(Vector3.up, rotationAxis);
                secondaryBody.transform.Rotate(rotationAxis, orbit.initialRotation);
            }

            if (!orbit.isStatic && primaryBody != null)
            {
                SetInitialMotion(initialMotion, primaryBody, secondaryBody);
            }
            else
            {
                initialMotion._initLinearDirection = Vector3.forward;
                initialMotion._initLinearSpeed = 0f;
            }

            return initialMotion;
        }

        private static void SetInitialMotion(InitialMotion initialMotion, AstroObject primaryBody, AstroObject secondaryBody)
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
                    var focalPointAO = focalPoint.GetComponent<AstroObject>();
                    SetMotionFromPrimary(focalPointAO, secondaryBody, secondaryBody as NHAstroObject, initialMotion);
                }
            }
            else if (primaryBody.GetGravityVolume())
            {
                SetMotionFromPrimary(primaryBody, secondaryBody, secondaryBody as NHAstroObject, initialMotion);
            }
            else
            {
                NHLogger.LogError($"Trying to put {secondaryBody.name} around {primaryBody.name} but found no primary gravity or focal point.");
            }
        }

        public static void SetMotionFromPrimary(AstroObject primaryBody, AstroObject secondaryBody, IOrbitalParameters orbit, InitialMotion initialMotion)
        {
            initialMotion.SetPrimaryBody(primaryBody.GetAttachedOWRigidbody());

            var primaryGravity = new Gravity(primaryBody.GetGravityVolume());
            var secondaryGravity = new Gravity(secondaryBody.GetGravityVolume());
            var velocity = orbit.GetOrbitalParameters(primaryGravity, secondaryGravity).InitialVelocity;

            var parentVelocity = primaryBody?.GetComponent<InitialMotion>()?.GetInitVelocity() ?? Vector3.zero;
            initialMotion._cachedInitVelocity = velocity + parentVelocity;
            initialMotion._isInitVelocityDirty = false;
        }

        private static void SetBinaryInitialMotion(AstroObject baryCenter, NHAstroObject primaryBody, NHAstroObject secondaryBody)
        {
            NHLogger.LogVerbose($"Setting binary initial motion [{primaryBody.name}] [{secondaryBody.name}]");

            var primaryGravity = new Gravity(primaryBody._gravityVolume);
            var secondaryGravity = new Gravity(secondaryBody._gravityVolume);

            // Might make binaries with binaries with binaries work 
            if (primaryBody.GetGravityVolume() == null)
            {
                primaryGravity = new Gravity(primaryBody.GetGravityVolume());
            }
            if (secondaryBody.GetGravityVolume() == null)
            {
                secondaryGravity = new Gravity(secondaryBody.GetGravityVolume());
            }

            // Update the positions
            var distance = secondaryBody.semiMajorAxis + primaryBody.semiMajorAxis;
            var m1 = primaryGravity.Mass;
            var m2 = secondaryGravity.Mass;

            var r1 = distance * m2 / (m1 + m2);
            var r2 = distance * m1 / (m1 + m2);

            var ecc = secondaryBody.eccentricity;
            var inc = secondaryBody.inclination;
            var arg = secondaryBody.argumentOfPeriapsis;
            var lon = secondaryBody.longitudeOfAscendingNode;
            var tru = secondaryBody.trueAnomaly;

            // Update their astro objects
            primaryBody.SetOrbitalParametersFromTrueAnomaly(ecc, r1, inc, arg - 180, lon, tru);
            secondaryBody.SetOrbitalParametersFromTrueAnomaly(ecc, r2, inc, arg, lon, tru);

            primaryBody.transform.position = baryCenter.transform.position + primaryBody.GetOrbitalParameters(primaryGravity, secondaryGravity).InitialPosition;
            secondaryBody.transform.position = baryCenter.transform.position + secondaryBody.GetOrbitalParameters(secondaryGravity, primaryGravity).InitialPosition;

            // Update the velocities

            // Two-body is equivalent to reduced mass orbiting the combined mass
            var reducedMass = 1f / ((1f / m1) + (1f / m2));
            var combinedMass = m1 + m2;

            var reducedMassGravity = new Gravity(reducedMass, primaryGravity.Power);
            var combinedMassGravity = new Gravity(combinedMass, primaryGravity.Power);

            var baryCenterVelocity = baryCenter?.GetComponent<InitialMotion>()?.GetInitVelocity() ?? Vector3.zero;

            // Doing the two body problem as a single one body problem gives the relative velocity of secondary to primary
            var reducedOrbit = OrbitalParameters.FromTrueAnomaly(
                combinedMassGravity,
                reducedMassGravity,
                ecc,
                distance,
                inc,
                arg,
                lon,
                tru
            );

            // mf uh sin theta = v1 / r1 = v2 / r2 bc same angular speed same angle I win!
            var v = reducedOrbit.InitialVelocity;
            var primaryVelocity = v / (1 + r2 / r1);
            var secondaryVelocity = primaryVelocity * r2 / r1;

            // Now we just set things
            var primaryInitialMotion = primaryBody.GetComponent<InitialMotion>();
            primaryInitialMotion._cachedInitVelocity = baryCenterVelocity - primaryVelocity;
            primaryInitialMotion._isInitVelocityDirty = false;

            var secondaryInitialMotion = secondaryBody.GetComponent<InitialMotion>();
            secondaryInitialMotion._cachedInitVelocity = baryCenterVelocity + secondaryVelocity;
            secondaryInitialMotion._isInitVelocityDirty = false;

            NHLogger.LogVerbose($"Binary Initial Motion: {m1}, {m2}, {r1}, {r2}, {primaryVelocity}, {secondaryVelocity}");
        }
    }
}
