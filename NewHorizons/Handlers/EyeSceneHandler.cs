using NewHorizons.Builder.General;
using NewHorizons.Components.EyeOfTheUniverse;
using NewHorizons.Components.Stars;
using NewHorizons.External.Modules.Props.EyeOfTheUniverse;
using NewHorizons.External.SerializableData;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class EyeSceneHandler
    {
        private static Dictionary<string, EyeTravelerData> _eyeTravelers = new();
        private static EyeMusicController _eyeMusicController;

        public static void Init()
        {
            _eyeTravelers.Clear();
            _eyeMusicController = null;
        }

        public static EyeMusicController GetMusicController()
        {
            return _eyeMusicController;
        }

        public static EyeTravelerData GetEyeTravelerData(string id)
        {
            if (_eyeTravelers.TryGetValue(id, out EyeTravelerData traveler))
            {
                return traveler;
            }
            return traveler;
        }

        public static EyeTravelerData CreateEyeTravelerData(string id)
        {
            if (_eyeTravelers.TryGetValue(id, out EyeTravelerData traveler))
            {
                return traveler;
            }
            traveler = new EyeTravelerData()
            {
                id = id,
                info = null,
                controller = null,
                loopAudioSource = null,
                finaleAudioSource = null,
                instrumentZones = new(),
                quantumInstruments = new(),
            };
            _eyeTravelers[traveler.id] = traveler;
            return traveler;
        }

        public static List<EyeTravelerData> GetActiveCustomEyeTravelers()
        {
            return _eyeTravelers.Values.Where(t => t.requirementsMet).ToList();
        }

        public static void OnSceneLoad()
        {
            // Create astro objects for eye and vessel because they didn't have them somehow.
            var eyeOfTheUniverse = SearchUtilities.Find("EyeOfTheUniverse_Body");
            var eyeSector = eyeOfTheUniverse.FindChild("Sector_EyeOfTheUniverse").GetComponent<Sector>();
            var eyeAO = eyeOfTheUniverse.AddComponent<EyeAstroObject>();
            var eyeBody = eyeOfTheUniverse.GetAttachedOWRigidbody();
            var eyeMarker = eyeOfTheUniverse.AddComponent<MapMarker>();
            var eyeSphere = eyeSector.GetComponent<SphereShape>();
            eyeSphere.SetLayer(Shape.Layer.Sector);
            eyeAO._owRigidbody = eyeBody;
            eyeAO._rootSector = eyeSector;
            eyeAO._gravityVolume = eyeSector.GetComponentInChildren<GravityVolume>();
            eyeAO._customName = "Eye Of The Universe";
            eyeAO._name = AstroObject.Name.Eye;
            eyeAO._type = AstroObject.Type.None;
            eyeAO.Register();
            eyeMarker._markerType = MapMarker.MarkerType.Sun;
            eyeMarker._labelID = UITextType.LocationEye_Cap;
            var eyeRFV = RFVolumeBuilder.Make(eyeOfTheUniverse, eyeBody, 400, new External.Modules.ReferenceFrameModule());

            var vessel = SearchUtilities.Find("Vessel_Body");
            var vesselSector = vessel.FindChild("Sector_VesselBridge").GetComponent<Sector>();
            var vesselAO = vessel.AddComponent<EyeAstroObject>();
            var vesselBody = vessel.GetAttachedOWRigidbody();
            var vesselMapMarker = vessel.AddComponent<MapMarker>();
            vesselAO._owRigidbody = vesselBody;
            vesselAO._primaryBody = eyeAO;
            eyeAO._satellite = vesselAO;
            vesselAO._rootSector = vesselSector;
            vesselAO._customName = "Vessel";
            vesselAO._name = AstroObject.Name.CustomString;
            vesselAO._type = AstroObject.Type.SpaceStation;
            vesselAO.Register();
            vesselMapMarker._markerType = MapMarker.MarkerType.Moon;
            vesselMapMarker._labelID = (UITextType)TranslationHandler.AddUI("Vessel", true);
            RFVolumeBuilder.Make(vessel, vesselBody, 600, new External.Modules.ReferenceFrameModule { localPosition = new MVector3(0, 0, -207.375f) });

            // Resize vessel sector so that the vessel is fully collidable.
            var vesselSectorTrigger = vesselSector.gameObject.FindChild("SectorTriggerVolume_VesselBridge");
            vesselSectorTrigger.transform.localPosition = new Vector3(0, 0, -207.375f);
            var vesselSectorTriggerBox = vesselSectorTrigger.GetComponent<BoxShape>();
            vesselSectorTriggerBox.size = new Vector3(600, 600, 600);
            vesselSectorTriggerBox.SetLayer(Shape.Layer.Sector);

            // Why were the vessel's lights inside the eye? Let's move them from the eye to vessel. Doesn't need to be moved positionally, only need to have the parent changed to vessel.
            var vesselPointlight = eyeSector.gameObject.FindChild("Pointlight_NOM_Vessel");
            vesselPointlight.transform.SetParent(vesselSector.transform, true);
            var vesselSpotlight = eyeSector.gameObject.FindChild("Spotlight_NOM_Vessel");
            vesselSpotlight.transform.SetParent(vesselSector.transform, true);
            var vesselAmbientLight = eyeSector.gameObject.FindChild("AmbientLight_Vessel");
            vesselAmbientLight.transform.SetParent(vesselSector.transform, true);

            // Add sector streaming to vessel just in case some one moves the vessel.
            var vesselStreaming = new GameObject("Sector_Streaming");
            vesselStreaming.transform.SetParent(vesselSector.transform, false);
            var vessel_ss = vesselStreaming.AddComponent<SectorStreaming>();
            vessel_ss._streamingGroup = eyeSector.gameObject.FindChild("StreamingGroup_EYE").GetComponent<StreamingGroup>();
            vessel_ss.SetSector(vesselSector);

            var eyeCoordinatePromptTrigger = vesselSector.GetComponentInChildren<EyeCoordinatePromptTrigger>(true);
            eyeCoordinatePromptTrigger._warpController = eyeCoordinatePromptTrigger.GetComponentInParent<VesselWarpController>();
            eyeCoordinatePromptTrigger.gameObject.SetActive(true);

            var solarSystemRoot = SearchUtilities.Find("SolarSystemRoot");

            // Disable forest and observatory so that a custom body doesn't accidentally pass through them. (they are 6.5km away from eye)
            // idk why mobius didn't add activation controllers for these

            var forestActivation = solarSystemRoot.AddComponent<EyeStateActivationController>();
            forestActivation._object = eyeSector.gameObject.FindChild("ForestOfGalaxies_Root");
            forestActivation._activeStates = new EyeState[]
            {
                    EyeState.Observatory,
                    EyeState.ZoomOut,
                    EyeState.ForestOfGalaxies,
                    EyeState.ForestIsDark
            };

            // mark

            var observatoryActivation = solarSystemRoot.AddComponent<EyeStateActivationController>();
            observatoryActivation._object = eyeSector.gameObject.FindChild("Sector_Observatory");
            observatoryActivation._activeStates = new EyeState[]
            {
                    EyeState.IntoTheVortex,
                    EyeState.Observatory,
                    EyeState.ZoomOut
            };

            var referenceFrameActivation = solarSystemRoot.AddComponent<EyeStateActivationController>();
            referenceFrameActivation._object = eyeRFV;
            referenceFrameActivation._activeStates = new EyeState[]
            {
                    EyeState.AboardVessel,
                    EyeState.WarpedToSurface,
                    EyeState.IntoTheVortex
            };

            // This component breaks the cosmic orb guy but only on Steam?
            // solarSystemRoot.AddComponent<EyeSunLightParamUpdater>();

            var distantSun = eyeSector.gameObject.FindChild("DistantSun/Directional light");
            var starController = distantSun.AddComponent<StarController>();
            var sunLight = distantSun.GetComponent<Light>();
            starController.Light = sunLight;
            var ambientLightFake = new GameObject("AmbientLightFake", typeof(Light));
            ambientLightFake.transform.SetParent(distantSun.transform, false);
            ambientLightFake.SetActive(false);
            var ambientLight = ambientLightFake.GetComponent<Light>();
            ambientLight.intensity = 0;
            ambientLight.range = 0;
            ambientLight.enabled = false;
            starController.AmbientLight = ambientLight;
            starController.Intensity = 0.2f;
            starController.SunColor = new Color(0.3569f, 0.7843f, 1, 1);
            var sunLightController = distantSun.AddComponent<SunLightController>();
            sunLightController._sunLight = sunLight;
            sunLightController._ambientLight = ambientLight;
            sunLightController.Initialize();
            var starLightController = distantSun.AddComponent<SunLightEffectsController>();
            starLightController.Awake();
            SunLightEffectsController.AddStar(starController);
            SunLightEffectsController.AddStarLight(sunLight);
        }

        public static void SetUpEyeCampfireSequence()
        {
            if (!GetActiveCustomEyeTravelers().Any()) return;

            _eyeMusicController = new GameObject("EyeMusicController").AddComponent<EyeMusicController>();

            var quantumCampsiteController = Object.FindObjectOfType<QuantumCampsiteController>();
            var cosmicInflationController = _eyeMusicController.CosmicInflationController;

            _eyeMusicController.RegisterFinaleSource(cosmicInflationController._travelerFinaleSource);

            foreach (var controller in quantumCampsiteController._travelerControllers)
            {
                _eyeMusicController.RegisterLoopSource(controller._signal.GetOWAudioSource());
            }

            foreach (var eyeTraveler in GetActiveCustomEyeTravelers())
            {
                if (eyeTraveler.controller != null)
                {
                    eyeTraveler.controller.gameObject.SetActive(false);

                    ArrayHelpers.Append(ref quantumCampsiteController._travelerControllers, eyeTraveler.controller);
                    eyeTraveler.controller.OnStartPlaying += quantumCampsiteController.OnTravelerStartPlaying;

                    ArrayHelpers.Append(ref cosmicInflationController._travelers, eyeTraveler.controller);
                    eyeTraveler.controller.OnStartPlaying += cosmicInflationController.OnTravelerStartPlaying;

                    ArrayHelpers.Append(ref cosmicInflationController._inflationObjects, eyeTraveler.controller.transform);
                }
                else
                {
                    NHLogger.LogError($"Missing Eye Traveler for ID \"{eyeTraveler.id}\"");
                }

                if (eyeTraveler.loopAudioSource != null)
                {
                    eyeTraveler.loopAudioSource.GetComponent<AudioSignal>().SetSignalActivation(false);
                    _eyeMusicController.RegisterLoopSource(eyeTraveler.loopAudioSource);
                }
                if (eyeTraveler.finaleAudioSource != null)
                {
                    _eyeMusicController.RegisterFinaleSource(eyeTraveler.finaleAudioSource);
                }

                foreach (var quantumInstrument in eyeTraveler.quantumInstruments)
                {
                    ArrayHelpers.Append(ref quantumInstrument._activateObjects, eyeTraveler.controller.gameObject);
                    ArrayHelpers.Append(ref quantumInstrument._deactivateObjects, eyeTraveler.instrumentZones.Select(z => z.gameObject));

                    var ancestorInstrumentZone = quantumInstrument.GetComponentInParent<InstrumentZone>();
                    if (ancestorInstrumentZone == null)
                    {
                        // Quantum instrument is not a child of an instrument zone, so treat it like its own zone
                        quantumInstrument.gameObject.SetActive(false);
                        ArrayHelpers.Append(ref quantumCampsiteController._instrumentZones, quantumInstrument.gameObject);

                        ArrayHelpers.Append(ref cosmicInflationController._inflationObjects, quantumInstrument.transform);
                    }
                }

                foreach (var instrumentZone in eyeTraveler.instrumentZones)
                {
                    instrumentZone.gameObject.SetActive(false);
                    ArrayHelpers.Append(ref quantumCampsiteController._instrumentZones, instrumentZone.gameObject);

                    ArrayHelpers.Append(ref cosmicInflationController._inflationObjects, instrumentZone.transform);
                }
            }

            UpdateTravelerPositions();
        }

        public static void UpdateTravelerPositions()
        {
            if (!GetActiveCustomEyeTravelers().Any()) return;

            var quantumCampsiteController = Object.FindObjectOfType<QuantumCampsiteController>();

            var travelers = new List<Transform>();

            var hasMetSolanum = quantumCampsiteController._hasMetSolanum;
            var hasMetPrisoner = quantumCampsiteController._hasMetPrisoner;

            // The order of the travelers in the base game differs depending on if the player has met both Solanum and the Prisoner or not.
            if (hasMetPrisoner && hasMetSolanum)
            {
                travelers.Add(quantumCampsiteController._travelerControllers[0].transform); // Riebeck
                travelers.Add(quantumCampsiteController._travelerControllers[5].transform); // Prisoner
                travelers.Add(quantumCampsiteController._travelerControllers[6].transform); // Esker
                travelers.Add(quantumCampsiteController._travelerControllers[1].transform); // Felspar
                travelers.Add(quantumCampsiteController._travelerControllers[3].transform); // Gabbro
                travelers.Add(quantumCampsiteController._travelerControllers[4].transform); // Solanum
                travelers.Add(quantumCampsiteController._travelerControllers[2].transform); // Chert
            }
            else
            {
                travelers.Add(quantumCampsiteController._travelerControllers[0].transform); // Riebeck
                travelers.Add(quantumCampsiteController._travelerControllers[2].transform); // Chert
                travelers.Add(quantumCampsiteController._travelerControllers[6].transform); // Esker
                travelers.Add(quantumCampsiteController._travelerControllers[1].transform); // Felspar
                travelers.Add(quantumCampsiteController._travelerControllers[3].transform); // Gabbro
                if (hasMetSolanum)
                    travelers.Add(quantumCampsiteController._travelerControllers[4].transform); // Solanum
                if (hasMetPrisoner)
                    travelers.Add(quantumCampsiteController._travelerControllers[5].transform); // Prisoner
            }

            // Custom travelers (starting at index 7, after Esker). We loop through the array instead of the list of custom travelers in case a non-NH mod added their own.
            for (int i = 7; i < quantumCampsiteController._travelerControllers.Length; i++)
            {
                var travelerInfo = GetActiveCustomEyeTravelers().FirstOrDefault(t => t.controller == quantumCampsiteController._travelerControllers[i]);
                var travelerName = travelerInfo?.info?.afterTraveler;
                if (travelerName.HasValue)
                {
                    InsertTravelerAfter(quantumCampsiteController, travelers, travelerInfo.info.afterTraveler.ToString(), quantumCampsiteController._travelerControllers[i].transform);
                }
                else
                {
                    travelers.Add(quantumCampsiteController._travelerControllers[i].transform);
                }
            }

            var radius = 2f + 0.2f * travelers.Count;
            var angle = Mathf.PI * 2f / travelers.Count;
            var index = 0;

            foreach (var traveler in travelers)
            {
                // Esker isn't at height 0 so we have to do all this
                var initialY = traveler.transform.position.y;
                var newPos = quantumCampsiteController.transform.TransformPoint(new Vector3(
                    Mathf.Cos(angle * index) * radius,
                    0f,
                    -Mathf.Sin(angle * index) * radius
                ));
                newPos.y = initialY;
                traveler.transform.position = newPos;
                var lookTarget = quantumCampsiteController.transform.position;
                lookTarget.y = newPos.y;
                traveler.transform.LookAt(lookTarget, traveler.transform.up);
                index++;
            }
        }

        private static void InsertTravelerAfter(QuantumCampsiteController campsite, List<Transform> travelers, string travelerName, Transform newTraveler)
        {
            if (travelerName == "Prisoner")
                travelerName = "Prisoner_Campfire";
            var existingTraveler = campsite._travelerControllers.FirstOrDefault(c => c.name == travelerName);
            if (existingTraveler != null)
            {
                var index = travelers.IndexOf(existingTraveler.transform);
                travelers.Insert(index + 1, newTraveler);
            }
            else
            {
                travelers.Add(newTraveler);
            }
        }

        public class EyeTravelerData
        {
            public string id;
            public EyeTravelerInfo info;
            public TravelerEyeController controller;
            public OWAudioSource loopAudioSource;
            public OWAudioSource finaleAudioSource;
            public List<QuantumInstrument> quantumInstruments = new();
            public List<InstrumentZone> instrumentZones = new();
            public bool requirementsMet;
        }
    }
}
