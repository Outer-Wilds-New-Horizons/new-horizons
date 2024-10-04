using NewHorizons.Components.Orbital;
using NewHorizons.External.Configs;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Builder.General
{
    public static class DetectorBuilder
    {
        private static List<SplashEffect> _splashEffects;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_splashEffects == null)
            {
                _splashEffects = new List<SplashEffect>();

                var cometDetector = SearchUtilities.Find("Comet_Body/Detector_CO")?.GetComponent<FluidDetector>();
                if (cometDetector != null)
                {
                    foreach (var splashEffect in cometDetector._splashEffects)
                    {
                        _splashEffects.Add(new SplashEffect
                        {
                            fluidType = splashEffect.fluidType,
                            ignoreSphereAligment = splashEffect.ignoreSphereAligment,
                            triggerEvent = splashEffect.triggerEvent,
                            minImpactSpeed = 15,
                            splashPrefab = splashEffect.splashPrefab
                        });
                    }
                }

                var islandDetector = SearchUtilities.Find("GabbroIsland_Body/Detector_GabbroIsland")?.GetComponent<FluidDetector>();
                if (islandDetector != null)
                {
                    foreach (var splashEffect in islandDetector._splashEffects)
                    {
                        _splashEffects.Add(new SplashEffect
                        {
                            fluidType = splashEffect.fluidType,
                            ignoreSphereAligment = splashEffect.ignoreSphereAligment,
                            triggerEvent = splashEffect.triggerEvent,
                            minImpactSpeed = 15,
                            splashPrefab = splashEffect.splashPrefab
                        });
                    }
                }

                var shipDetector = SearchUtilities.Find("Ship_Body/ShipDetector")?.GetComponent<FluidDetector>();
                if (shipDetector != null)
                {
                    foreach (var splashEffect in shipDetector._splashEffects)
                    {
                        if (splashEffect.fluidType == FluidVolume.Type.SAND)
                            _splashEffects.Add(new SplashEffect
                            {
                                fluidType = splashEffect.fluidType,
                                ignoreSphereAligment = splashEffect.ignoreSphereAligment,
                                triggerEvent = splashEffect.triggerEvent,
                                minImpactSpeed = 15,
                                splashPrefab = splashEffect.splashPrefab
                            });
                    }
                }
            }
        }

        public static GameObject Make(GameObject planetGO, OWRigidbody OWRB, AstroObject primaryBody, AstroObject astroObject, PlanetConfig config)
        {
            InitPrefabs();

            GameObject detectorGO = new GameObject("FieldDetector");
            detectorGO.SetActive(false);
            detectorGO.transform.parent = planetGO.transform;
            detectorGO.transform.localPosition = Vector3.zero;
            detectorGO.layer = Layer.BasicDetector;

            ConstantForceDetector forceDetector = detectorGO.AddComponent<ConstantForceDetector>();
            forceDetector._inheritElement0 = true;
            OWRB.RegisterAttachedForceDetector(forceDetector);

            // For falling into sun
            if (config.Base.hasFluidDetector && config.Star == null && config.FocalPoint == null)
            {
                detectorGO.layer = Layer.AdvancedDetector;

                var fluidDetector = detectorGO.AddComponent<DynamicFluidDetector>();
                var sphereCollider = detectorGO.AddComponent<SphereCollider>();
                sphereCollider.radius = config.Base.surfaceSize;

                var owCollider = detectorGO.AddComponent<OWCollider>();

                fluidDetector._collider = sphereCollider;

                OWRB.RegisterAttachedFluidDetector(fluidDetector);

                fluidDetector._splashEffects = _splashEffects.ToArray();
            }

            if (!config.Orbit.isStatic) SetDetector(primaryBody, astroObject, forceDetector);

            detectorGO.SetActive(true);
            return detectorGO;
        }

        public static void SetDetector(AstroObject primaryBody, AstroObject astroObject, ConstantForceDetector forceDetector)
        {
            var binaryFocalPoint = primaryBody?.gameObject?.GetComponent<BinaryFocalPoint>();
            var parentGravityVolume = primaryBody?.GetAttachedOWRigidbody()?.GetAttachedGravityVolume();

            if (binaryFocalPoint != null)
            {
                if (astroObject.GetCustomName().Equals(binaryFocalPoint.PrimaryName))
                {
                    binaryFocalPoint.Primary = astroObject;
                    if (binaryFocalPoint.Secondary != null)
                    {
                        var secondaryRB = binaryFocalPoint.Secondary.GetAttachedOWRigidbody();
                        SetBinaryForceDetectableFields(binaryFocalPoint, forceDetector, (ConstantForceDetector)secondaryRB.GetAttachedForceDetector());
                    }
                }
                else if (astroObject.GetCustomName().Equals(binaryFocalPoint.SecondaryName))
                {
                    binaryFocalPoint.Secondary = astroObject;
                    if (binaryFocalPoint.Primary != null)
                    {
                        var primaryRB = binaryFocalPoint.Primary.GetAttachedOWRigidbody();
                        SetBinaryForceDetectableFields(binaryFocalPoint, (ConstantForceDetector)primaryRB.GetAttachedForceDetector(), forceDetector);
                    }
                }
                else
                {
                    // It's a planet
                    if (binaryFocalPoint.Primary != null && binaryFocalPoint.Secondary != null)
                    {
                        if (!parentGravityVolume) NHLogger.LogError($"{astroObject?.name} trying to orbit {primaryBody?.name}, which has no gravity volume!");
                        forceDetector._detectableFields = parentGravityVolume ? new ForceVolume[] { parentGravityVolume } : new ForceVolume[0];
                    }
                }
            }
            else
            {
                if (!parentGravityVolume) NHLogger.LogError($"{astroObject?.name} trying to orbit {primaryBody?.name}, which has no gravity volume!");
                forceDetector._detectableFields = parentGravityVolume ? new ForceVolume[] { parentGravityVolume } : new ForceVolume[0];
            }
        }

        private static void SetBinaryForceDetectableFields(BinaryFocalPoint point, ConstantForceDetector primaryCFD, ConstantForceDetector secondaryCFD)
        {
            NHLogger.Log($"Setting up binary focal point for {point.name}");

            var primary = point.Primary;
            var secondary = point.Secondary;

            // Binaries have to use the same gravity exponent
            var primaryGV = primary.GetGravityVolume();
            var secondaryGV = secondary.GetGravityVolume();

            if (primaryGV && secondaryGV && primaryGV._falloffType != secondaryGV._falloffType)
            {
                NHLogger.LogError($"Binaries must have the same gravity falloff! {primaryGV._falloffType} != {secondaryGV._falloffType}");
                return;
            }

            var pointForceDetector = point.GetAttachedOWRigidbody().GetAttachedForceDetector();

            // Set detectable fields
            if (!secondaryGV) NHLogger.LogError($"{point.PrimaryName} trying to orbit {point.SecondaryName}, which has no gravity volume!");
            primaryCFD._detectableFields = secondaryGV ? new ForceVolume[] { secondaryGV } : new ForceVolume[0];
            primaryCFD._inheritDetector = pointForceDetector;
            primaryCFD._activeInheritedDetector = pointForceDetector;
            primaryCFD._inheritElement0 = false;

            if (!primaryGV) NHLogger.LogError($"{point.SecondaryName} trying to orbit {point.PrimaryName}, which has no gravity volume!");
            secondaryCFD._detectableFields = primaryGV ? new ForceVolume[] { primaryGV } : new ForceVolume[0];
            secondaryCFD._inheritDetector = pointForceDetector;
            secondaryCFD._activeInheritedDetector = pointForceDetector;
            secondaryCFD._inheritElement0 = false;
        }
    }
}
