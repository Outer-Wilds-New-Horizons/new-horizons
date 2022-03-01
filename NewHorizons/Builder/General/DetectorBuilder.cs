using NewHorizons.Builder.Orbital;
using NewHorizons.External;
using NewHorizons.Components.Orbital;
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
                            var fakeBarycenterGravityVolume = binaryFocalPoint.FakeMassBody.GetComponent<AstroObject>().GetGravityVolume();
                            forceDetector.SetValue("_detectableFields", new ForceVolume[] { fakeBarycenterGravityVolume });
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

            if (primaryGV._falloffType != secondaryGV._falloffType)
            {
                Logger.LogError($"Binaries must have the same gravity falloff! {primaryGV._falloffType} != {secondaryGV._falloffType}");
                return;
            }

            var pointForceDetector = point.GetAttachedOWRigidbody().GetAttachedForceDetector();

            // Set detectable fields
            primaryCFD.SetValue("_detectableFields", new ForceVolume[] { secondaryGV });
            primaryCFD.SetValue("_inheritDetector", pointForceDetector);
            primaryCFD.SetValue("_activeInheritedDetector", pointForceDetector);
            primaryCFD.SetValue("_inheritElement0", false);

            secondaryCFD.SetValue("_detectableFields", new ForceVolume[] { primaryGV });
            secondaryCFD.SetValue("_inheritDetector", pointForceDetector);
            secondaryCFD.SetValue("_activeInheritedDetector", pointForceDetector);
            secondaryCFD.SetValue("_inheritElement0", false);

            foreach(var planet in planets)
            {
                var planetCFD = planet.GetAttachedOWRigidbody().GetAttachedForceDetector() as ConstantForceDetector;
                planetCFD._detectableFields = new ForceVolume[] { primaryGV, secondaryGV };
                planetCFD._inheritDetector = pointForceDetector;
                planetCFD._activeInheritedDetector = pointForceDetector;
                planetCFD._inheritElement0 = false;
            }
        }
    }
}
