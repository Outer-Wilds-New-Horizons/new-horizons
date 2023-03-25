using NewHorizons.Builder.General;
using NewHorizons.Components;
using NewHorizons.Components.Stars;
using NewHorizons.External.Modules.SerializableData;
using NewHorizons.Utility;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class EyeSceneHandler
    {
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
            vesselMapMarker._labelID = (UITextType)TranslationHandler.AddUI("Vessel");
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
    }
}
