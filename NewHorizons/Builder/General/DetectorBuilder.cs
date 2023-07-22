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
            if (!config.Base.invulnerableToSun && config.Star == null && config.FocalPoint == null)
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
                        forceDetector._detectableFields = new ForceVolume[] { parentGravityVolume };
                    }
                }
            }
            else
            {
                forceDetector._detectableFields = new ForceVolume[] { parentGravityVolume };
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

            if (primaryGV._falloffType != secondaryGV._falloffType)
            {
                NHLogger.LogError($"Binaries must have the same gravity falloff! {primaryGV._falloffType} != {secondaryGV._falloffType}");
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
        }
    }
}