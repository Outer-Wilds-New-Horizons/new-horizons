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
using NewHorizons.External.Configs;

namespace NewHorizons.Builder.General
{
    static class DetectorBuilder
    {
        public static GameObject Make(GameObject body, OWRigidbody OWRB, AstroObject primaryBody, AstroObject astroObject, IPlanetConfig config)
        {
            GameObject detectorGO = new GameObject("FieldDetector");
            detectorGO.SetActive(false);
            detectorGO.transform.parent = body.transform;
            detectorGO.transform.localPosition = Vector3.zero;
            detectorGO.layer = LayerMask.NameToLayer("BasicDetector");

            ConstantForceDetector forceDetector = detectorGO.AddComponent<ConstantForceDetector>();
            forceDetector._inheritElement0 = true;
            OWRB.RegisterAttachedForceDetector(forceDetector);

            // For falling into sun
            if(!config.Base.InvulnerableToSun && config.Star == null && config.FocalPoint == null)
            {
                detectorGO.layer = LayerMask.NameToLayer("AdvancedDetector");

                var fluidDetector = detectorGO.AddComponent<DynamicFluidDetector>();
                var sphereCollider = detectorGO.AddComponent<SphereCollider>();
                sphereCollider.radius = config.Base.SurfaceSize;

                var owCollider = detectorGO.AddComponent<OWCollider>();
                
                fluidDetector._collider = sphereCollider;

                // Could copy the splash from the interloper as well some day
            }

            GravityVolume parentGravityVolume = primaryBody?.GetAttachedOWRigidbody()?.GetAttachedGravityVolume();
            if (parentGravityVolume != null)
            {
                forceDetector._detectableFields = new ForceVolume[] { parentGravityVolume };
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
                            SetBinaryForceDetectableFields(binaryFocalPoint, forceDetector, (ConstantForceDetector)secondaryRB.GetAttachedForceDetector(), OWRB, secondaryRB);
                        }
                    }
                    else if (astroObject.GetCustomName().Equals(binaryFocalPoint.SecondaryName))
                    {
                        binaryFocalPoint.Secondary = astroObject;
                        if (binaryFocalPoint.Primary != null)
                        {
                            var primaryRB = binaryFocalPoint.Primary.GetAttachedOWRigidbody();
                            SetBinaryForceDetectableFields(binaryFocalPoint, (ConstantForceDetector)primaryRB.GetAttachedForceDetector(), forceDetector, primaryRB, OWRB);
                        }
                    }
                    else
                    {
                        // It's a planet
                        binaryFocalPoint.Planets.Add(astroObject);
                        if(binaryFocalPoint.Primary != null && binaryFocalPoint.Secondary != null)
                        {
                            var fakeBarycenterGravityVolume = binaryFocalPoint.FakeMassBody.GetComponent<AstroObject>().GetGravityVolume();
                            forceDetector._detectableFields = new ForceVolume[] { fakeBarycenterGravityVolume };
                        }
                    }
                }
            }

            detectorGO.SetActive(true);
            return detectorGO;
        }

        private static void SetBinaryForceDetectableFields(BinaryFocalPoint point, ConstantForceDetector primaryCFD, ConstantForceDetector secondaryCFD, OWRigidbody primaryRB, OWRigidbody secondaryRB)
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
            primaryCFD._detectableFields = new ForceVolume[] { secondaryGV };
            primaryCFD._inheritDetector = pointForceDetector;
            primaryCFD._activeInheritedDetector = pointForceDetector;
            primaryCFD._inheritElement0 = false;

            secondaryCFD._detectableFields = new ForceVolume[] { primaryGV };
            secondaryCFD._inheritDetector = pointForceDetector;
            secondaryCFD._activeInheritedDetector = pointForceDetector;
            secondaryCFD._inheritElement0 = false;

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
