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

            GravityVolume parentGravityVolume = primaryBody.GetAttachedOWRigidbody().GetAttachedGravityVolume();
            if (parentGravityVolume != null)
            {
                forceDetector.SetValue("_detectableFields", new ForceVolume[] { parentGravityVolume });
            }
            else if (astroObject != null)
            {
                // It's probably a focal point (or its just broken)
                var binaryFocalPoint = primaryBody.gameObject.GetComponent<BinaryFocalPoint>();
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
            var primaryAO = primary.GetComponent<ParameterizedAstroObject>();
            var secondaryAO = secondary.GetComponent<ParameterizedAstroObject>();
            if (primaryAO == null||secondaryAO == null)
            {
                Logger.LogError($"Couldn't find ParameterizedAstroObject for body {primaryRB.name} or {secondaryRB.name}");
                return;
            }

            float ecc = primaryAO.Eccentricity;
            float i = primaryAO.Inclination;
            float l = primaryAO.LongitudeOfAscendingNode;
            float p = primaryAO.ArgumentOfPeriapsis;

            // Update speeds
            var m1 = Gm1 / GravityVolume.GRAVITATIONAL_CONSTANT;
            var m2 = Gm2 / GravityVolume.GRAVITATIONAL_CONSTANT;
            var reducedMass = m1 * m2 / (m1 + m2);
            var totalMass = m1 + m2;

            var r = separation.magnitude;

            Logger.Log($"Primary AO: [{primaryAO}], Secondary AO: [{secondaryAO}]");

            // Start them off at their periapsis
            primaryAO.SetKeplerCoordinatesFromTrueAnomaly(ecc, r1 / (1f - ecc), i, l, p, 0);
            secondaryAO.SetKeplerCoordinatesFromTrueAnomaly(ecc, r2 / (1f - ecc), i, l, p, 180);

            // Make sure positions are right
            var gravity = new OrbitalHelper.Gravity(exponent, totalMass);

            var primaryCat = OrbitalHelper.GetCartesian(gravity, primaryAO);
            var secondaryCat = OrbitalHelper.GetCartesian(gravity, secondaryAO);

            primary.transform.position = point.transform.position + primaryCat.Item1;
            secondary.transform.position = point.transform.position + secondaryCat.Item1;

            Logger.Log($"Primary AO: [{primaryAO}], Secondary AO: [{secondaryAO}]");

            // Finally we update the speeds
            var v1 = primaryCat.Item2;
            var v2 = primaryCat.Item2;

            var focalPointMotion = point.gameObject.GetComponent<InitialMotion>();
            var focalPointVelocity = focalPointMotion == null ? Vector3.zero : focalPointMotion.GetInitVelocity();

            var primaryInitialMotion = primary.gameObject.GetComponent<InitialMotion>();
            primaryInitialMotion.SetValue("_initLinearDirection", primary.gameObject.transform.InverseTransformDirection(v1.normalized));
            primaryInitialMotion.SetValue("_initLinearSpeed", v1.magnitude);

            var secondaryInitialMotion = secondary.gameObject.GetComponent<InitialMotion>();
            secondaryInitialMotion.SetValue("_initLinearDirection", secondary.gameObject.transform.InverseTransformDirection(v2.normalized));
            secondaryInitialMotion.SetValue("_initLinearSpeed", v2.magnitude);

            // InitialMotion already set its speed so we overwrite that
            if (!primaryInitialMotion.GetValue<bool>("_isInitVelocityDirty"))
            {
                Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => primaryRB.SetVelocity(v1 + focalPointVelocity));
            }
            if (!secondaryInitialMotion.GetValue<bool>("_isInitVelocityDirty"))
            {
                Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => secondaryRB.SetVelocity(v2 + focalPointVelocity));
            }

            // If they have tracking orbits set the period
            var period = 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(r, exponent + 1) / (GravityVolume.GRAVITATIONAL_CONSTANT * totalMass));

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
