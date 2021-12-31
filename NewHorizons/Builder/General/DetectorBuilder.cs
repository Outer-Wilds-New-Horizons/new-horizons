using NewHorizons.Builder.Orbital;
using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    static class DetectorBuilder
    {
        public static GameObject Make(GameObject body, OWRigidbody OWRB, AstroObject primaryBody, AstroObject astroObject, bool inherit = true)
        {
            GameObject detectorGO = new GameObject("FieldDetector");
            detectorGO.SetActive(false);
            detectorGO.transform.parent = body.transform;
            detectorGO.transform.localPosition = Vector3.zero;
            detectorGO.layer = 20;

            ConstantForceDetector forceDetector = detectorGO.AddComponent<ConstantForceDetector>();
            forceDetector.SetValue("_inheritElement0", inherit);
            OWRB.RegisterAttachedForceDetector(forceDetector);

            GravityVolume parentGravityVolume = primaryBody?.GetAttachedOWRigidbody()?.GetAttachedGravityVolume();
            if (parentGravityVolume != null)
            {
                forceDetector.SetValue("_detectableFields", new ForceVolume[] { parentGravityVolume });
            }
            else if (astroObject != null)
            {
                // It's probably a focal point (or its just broken)
                var binaryFocalPoint = primaryBody?.gameObject?.GetComponent<BinaryFocalPoint>();
                if(binaryFocalPoint != null)
                {
                    if(astroObject.GetCustomName().Equals(binaryFocalPoint.PrimaryName)) {
                        binaryFocalPoint.Primary = astroObject;
                        if (binaryFocalPoint.Secondary != null)
                        {
                            var secondaryRB = binaryFocalPoint.Secondary.GetAttachedOWRigidbody();
                            SetBinaryForceDetectableFields(binaryFocalPoint, forceDetector, secondaryRB.GetAttachedForceDetector(), OWRB, secondaryRB);
                        }
                    }
                    else if (astroObject.GetCustomName().Equals(binaryFocalPoint.SecondaryName))
                    {
                        binaryFocalPoint.Secondary = astroObject;
                        if (binaryFocalPoint.Primary != null)
                        {
                            var primaryRB = binaryFocalPoint.Primary.GetAttachedOWRigidbody();
                            SetBinaryForceDetectableFields(binaryFocalPoint, primaryRB.GetAttachedForceDetector(), forceDetector, primaryRB, OWRB);
                        }
                    }
                    else
                    {
                        // It's a planet
                        binaryFocalPoint.Planets.Add(astroObject);
                        if(binaryFocalPoint.Primary != null && binaryFocalPoint.Secondary != null)
                        {
                            var primaryGravityVolume = binaryFocalPoint.Primary.GetGravityVolume();
                            var secondaryGravityVolume = binaryFocalPoint.Secondary.GetGravityVolume();
                            forceDetector.SetValue("_detectableFields", new ForceVolume[] { primaryGravityVolume, secondaryGravityVolume });
                        }
                    }
                }
            }

            detectorGO.SetActive(true);
            return detectorGO;
        }

        private static void SetBinaryForceDetectableFields(BinaryFocalPoint point, ForceDetector primaryCFD, ForceDetector secondaryCFD, OWRigidbody primaryRB, OWRigidbody secondaryRB)
        {
            Logger.Log($"Setting up binary focal point for {point.name}");

            var primary = point.Primary;
            var secondary = point.Secondary;
            var planets = point.Planets;

            
            // Binaries have to use the same gravity exponent
            var primaryGV = primary.GetGravityVolume();
            var secondaryGV = secondary.GetGravityVolume();

            if (primaryGV.GetFalloffType() != secondaryGV.GetFalloffType())
            {
                Logger.LogError($"Binaries must have the same gravity falloff! {primaryGV.GetFalloffType()} != {secondaryGV.GetFalloffType()}");
                return;
            }

            // Have to use fall off type rn instead of exponent since it the exponent doesnt update until the Awake method I think
            var exponent = 0f;
            if (primaryGV.GetFalloffType() == OrbitalHelper.FalloffType.linear) exponent = 1f;
            if (primaryGV.GetFalloffType() == OrbitalHelper.FalloffType.inverseSquared) exponent = 2f;

            // Very specific distance between them
            Vector3 separation = primary.transform.position - secondary.transform.position;
            var Gm1 = primaryGV.GetStandardGravitationalParameter();
            var Gm2 = secondaryGV.GetStandardGravitationalParameter();

            // One of the two is zero if it just loaded
            if (Gm1 == 0) Gm1 = GravityVolume.GRAVITATIONAL_CONSTANT * primaryGV.GetValue<float>("_surfaceAcceleration") * Mathf.Pow(primaryGV.GetValue<float>("_upperSurfaceRadius"), exponent) / 0.001f;
            if (Gm2 == 0) Gm2 = GravityVolume.GRAVITATIONAL_CONSTANT * secondaryGV.GetValue<float>("_surfaceAcceleration") * Mathf.Pow(secondaryGV.GetValue<float>("_upperSurfaceRadius"), exponent) / 0.001f;

            float r1 = separation.magnitude * Gm2 / (Gm1 + Gm2);
            float r2 = separation.magnitude * Gm1 / (Gm1 + Gm2);

            primary.transform.position = point.transform.position + r1 * separation.normalized;
            secondary.transform.position = point.transform.position - r2 * separation.normalized;

            // Set detectable fields
            primaryCFD.SetValue("_detectableFields", new ForceVolume[] { secondaryGV });
            primaryCFD.SetValue("_inheritDetector", point.GetAttachedOWRigidbody().GetAttachedForceDetector());
            primaryCFD.SetValue("_activeInheritedDetector", point.GetAttachedOWRigidbody().GetAttachedForceDetector());
            primaryCFD.SetValue("_inheritElement0", false);

            secondaryCFD.SetValue("_detectableFields", new ForceVolume[] { primaryGV });
            secondaryCFD.SetValue("_inheritDetector", point.GetAttachedOWRigidbody().GetAttachedForceDetector());
            secondaryCFD.SetValue("_activeInheritedDetector", point.GetAttachedOWRigidbody().GetAttachedForceDetector());
            secondaryCFD.SetValue("_inheritElement0", false);

            // They must have the same eccentricity
            var parameterizedAstroObject = primary.GetComponent<ParameterizedAstroObject>();
            var parameterizedAstroObject2 = secondary.GetComponent<ParameterizedAstroObject>();

            float ecc = 0;
            float i = 0;
            float l = 0;
            float p = 0;
            if (parameterizedAstroObject != null)
            {
                ecc = parameterizedAstroObject.keplerElements.Eccentricity;
                i = parameterizedAstroObject.keplerElements.Inclination;
                l = parameterizedAstroObject.keplerElements.LongitudeOfAscendingNode;
                p = parameterizedAstroObject.keplerElements.ArgumentOfPeriapsis;
            }

            // Update speeds
            var direction = Vector3.Cross(separation, Vector3.up).normalized;
            if (direction.sqrMagnitude == 0) direction = Vector3.left;

            var m1 = Gm1 / GravityVolume.GRAVITATIONAL_CONSTANT;
            var m2 = Gm2 / GravityVolume.GRAVITATIONAL_CONSTANT;
            var reducedMass = m1 * m2 / (m1 + m2);
            var totalMass = m1 + m2;

            var r = separation.magnitude;

            // Start them off at their periapsis
            var primaryKeplerElements = KeplerElements.FromTrueAnomaly(ecc, r1 / (1f - ecc), i, l, p, 0);
            var secondaryKeplerElements = KeplerElements.FromTrueAnomaly(ecc, r2 / (1f - ecc), i, l, p, 180);

            // Maybe we'll need these orbital parameters later
            if(parameterizedAstroObject != null) parameterizedAstroObject.keplerElements = primaryKeplerElements;
            if(parameterizedAstroObject2 != null) parameterizedAstroObject2.keplerElements = secondaryKeplerElements;

            // Finally we update the speeds
            float v = Mathf.Sqrt(GravityVolume.GRAVITATIONAL_CONSTANT * totalMass * (1 - ecc * ecc) / Mathf.Pow(r, exponent - 1));
            var v2 = v / (1f + (m2 / m1));
            var v1 = v - v2;

            // Rotate 
            var rot = Quaternion.AngleAxis(l + p + 180f, Vector3.up);
            var incAxis = Quaternion.AngleAxis(l, Vector3.up) * Vector3.left;
            var incRot = Quaternion.AngleAxis(i, incAxis);

            //Do this next tick for... reasons?
            var focalPointMotion = point.gameObject.GetComponent<InitialMotion>();
            var focalPointVelocity = focalPointMotion == null ? Vector3.zero : focalPointMotion.GetInitVelocity();

            var d1 = Vector3.Cross(OrbitalHelper.RotateTo(Vector3.up, primaryKeplerElements), separation.normalized);
            var d2 = Vector3.Cross(OrbitalHelper.RotateTo(Vector3.up, primaryKeplerElements), separation.normalized);

            var primaryInitialMotion = primary.gameObject.GetComponent<InitialMotion>();
            primaryInitialMotion.SetValue("_initLinearDirection", d1);
            primaryInitialMotion.SetValue("_initLinearSpeed", v1);

            var secondaryInitialMotion = secondary.gameObject.GetComponent<InitialMotion>();
            secondaryInitialMotion.SetValue("_initLinearDirection", d2);
            secondaryInitialMotion.SetValue("_initLinearSpeed", -v2);

            // InitialMotion already set its speed so we overwrite that
            if (!primaryInitialMotion.GetValue<bool>("_isInitVelocityDirty"))
            {
                Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => primaryRB.SetVelocity(d1 * v1 + focalPointVelocity));
            }
            if (!secondaryInitialMotion.GetValue<bool>("_isInitVelocityDirty"))
            {
                Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => primaryRB.SetVelocity(d2 * -v2 + focalPointVelocity));
            }

            // If they have tracking orbits set the period
            var period = 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(r, exponent + 1) / (GravityVolume.GRAVITATIONAL_CONSTANT * totalMass));

            if (exponent == 1) period /= 3f;

            // Only one of these won't be null, the other one gets done next tick
            var trackingOrbitPrimary = primary.GetComponentInChildren<TrackingOrbitLine>();
            if (trackingOrbitPrimary != null)
            {
                trackingOrbitPrimary.TrailTime = period;
            }

            var trackingOrbitSecondary = secondary.GetComponentInChildren<TrackingOrbitLine>();
            if (trackingOrbitSecondary != null)
            {
                trackingOrbitSecondary.TrailTime = period;
            }
        }
    }
}
