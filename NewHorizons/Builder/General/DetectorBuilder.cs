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
        public static void Make(GameObject body, OWRigidbody OWRB, AstroObject primaryBody, AstroObject astroObject)
        {
            GameObject detectorGO = new GameObject("FieldDetector");
            detectorGO.SetActive(false);
            detectorGO.transform.parent = body.transform;
            detectorGO.transform.localPosition = Vector3.zero;
            detectorGO.layer = 20;

            ConstantForceDetector forceDetector = detectorGO.AddComponent<ConstantForceDetector>();
            forceDetector.SetValue("_inheritElement0", true);
            OWRB.RegisterAttachedForceDetector(forceDetector);

            GravityVolume parentGravityVolume = primaryBody.GetAttachedOWRigidbody().GetAttachedGravityVolume();
            if (parentGravityVolume != null)
            {
                forceDetector.SetValue("_detectableFields", new ForceVolume[] { parentGravityVolume });
            }
            else
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
        }

        private static void SetBinaryForceDetectableFields(BinaryFocalPoint point, ForceDetector primaryCFD, ForceDetector secondaryCFD, OWRigidbody primaryRB, OWRigidbody secondaryRB)
        {
            Logger.Log($"Setting up binary focal point for {point.name}");

            var primary = point.Primary;
            var secondary = point.Secondary;
            var planets = point.Planets;

            
            // Binaries have to use inverse square gravity so overwrite it now
            var primaryGV = primary.GetGravityVolume();
            //primaryGV.SetValue("_falloffType", primaryGV.GetType().GetNestedType("FalloffType", BindingFlags.NonPublic).GetField("inverseSquared").GetValue(primaryGV));
            //primaryGV.SetValue("_falloffExponent", 2);

            var secondaryGV = secondary.GetGravityVolume();
            //secondaryGV.SetValue("_falloffType", secondaryGV.GetType().GetNestedType("FalloffType", BindingFlags.NonPublic).GetField("inverseSquared").GetValue(secondaryGV));
            //secondaryGV.SetValue("_falloffExponent", 2);
            

            // Very specific distance between them
            Vector3 separation = primary.transform.position - secondary.transform.position;
            var Gm1 = primaryGV.GetStandardGravitationalParameter();
            var Gm2 = secondaryGV.GetStandardGravitationalParameter();
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

            // Update speeds
            var direction = Vector3.Cross(separation, Vector3.up).normalized;
            var m1 = primaryRB.GetMass();
            var m2 = secondaryRB.GetMass();
            var reducedMass = m1 * m2 / (m1 + m2);
            var r = separation.magnitude;
            float v1 = OrbitalHelper.GetOrbitalVelocity(r, new Gravity(secondaryGV.GetFalloffExponent(), reducedMass), KeplerElements.FromTrueAnomaly(0.5f, r, 0, 0, 0, 0));
            float v2 = OrbitalHelper.GetOrbitalVelocity(r, new Gravity(primaryGV.GetFalloffExponent(), reducedMass), KeplerElements.FromTrueAnomaly(0.5f, r, 0, 0, 0, 180));

            Logger.Log($"Speed: {v1} {v2}");

            primaryRB.UpdateCenterOfMass();
            primaryRB.AddVelocityChange(direction * v1);
            secondaryRB.UpdateCenterOfMass();                                       
            secondaryRB.AddVelocityChange(direction * -v2);



            /*
            //Hacky but whatever
            var primaryInitialMotion = primary.gameObject.GetComponent<InitialMotion>();
            if(primaryInitialMotion.GetValue<bool>("_isInitVelocityDirty"))
                primaryInitialMotion.SetValue("_cachedInitVelocity", OWPhysics.CalculateOrbitVelocity(secondaryRB, primaryRB, 0f));
            else
                primaryInitialMotion.SetPrimaryBody(secondaryRB);
            
            var secondaryInitialMotion = primary.gameObject.GetComponent<InitialMotion>();
            if (secondaryInitialMotion.GetValue<bool>("_isInitVelocityDirty"))
                secondaryInitialMotion.SetValue("_cachedInitVelocity", OWPhysics.CalculateOrbitVelocity(primaryRB, secondaryRB, 0f));
            else
                secondaryInitialMotion.SetPrimaryBody(primaryRB);
            */
        }
    }
}
