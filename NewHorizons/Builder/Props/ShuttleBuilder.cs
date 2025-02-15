using NewHorizons.Components;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.Shuttle;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class ShuttleBuilder
    {
        private static GameObject _prefab;
        private static GameObject _bodyPrefab;

        public static Dictionary<NomaiShuttleController.ShuttleID, NomaiShuttleController> Shuttles { get; } = new();

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = SearchUtilities.Find("QuantumMoon_Body/Sector_QuantumMoon/QuantumShuttle/Prefab_NOM_Shuttle")?.InstantiateInactive()?.Rename("Prefab_QM_Shuttle")?.DontDestroyOnLoad();
                NomaiShuttleController shuttleController = _prefab.GetComponent<NomaiShuttleController>();
                NHShuttleController nhShuttleController = _prefab.AddComponent<NHShuttleController>();
                nhShuttleController._exteriorSector = _prefab.FindChild("Sector_Shuttle").GetComponent<Sector>();
                nhShuttleController._interiorSector = _prefab.FindChild("Sector_NomaiShuttleInterior").GetComponent<Sector>();
                nhShuttleController._shuttleBody = shuttleController._shuttleBody;
                nhShuttleController._retrievalLength = shuttleController._retrievalLength;
                nhShuttleController._launchSlot = shuttleController._launchSlot;
                nhShuttleController._retrieveSlot = shuttleController._retrieveSlot;
                nhShuttleController._landSlot = shuttleController._landSlot;
                nhShuttleController._triggerVolume = shuttleController._triggerVolume;
                nhShuttleController._beamResetVolume = shuttleController._beamResetVolume;
                nhShuttleController._tractorBeam = shuttleController._tractorBeam;
                nhShuttleController._forceVolume = shuttleController._forceVolume;
                nhShuttleController._exteriorCollisionGroup = shuttleController._exteriorCollisionGroup;
                nhShuttleController._exteriorColliderRoot = shuttleController._exteriorColliderRoot;
                nhShuttleController._landingBeamRoot = shuttleController._landingBeamRoot;
                nhShuttleController._warpEffect = _prefab.GetComponentInChildren<SingularityWarpEffect>();
                nhShuttleController._exteriorLegColliders = shuttleController._exteriorLegColliders;
                nhShuttleController._id = NomaiShuttleController.ShuttleID.HourglassShuttle;
                nhShuttleController._cannon = null;
                GameObject slots = _prefab.FindChild("Sector_NomaiShuttleInterior/Interactibles_NomaiShuttleInterior/ControlPanel/Slots");
                GameObject neutralSlotObject = new GameObject("Slot_Neutral", typeof(NomaiInterfaceSlot));
                neutralSlotObject.transform.SetParent(slots.transform, false);
                neutralSlotObject.transform.localPosition = new Vector3(-0.0153f, -0.2386f, 0.0205f);
                neutralSlotObject.transform.localRotation = Quaternion.identity;
                NomaiInterfaceSlot neutralSlot = neutralSlotObject.GetComponent<NomaiInterfaceSlot>();
                neutralSlot._exitRadius = 0.1f;
                neutralSlot._radius = 0.05f;
                neutralSlot._attractive = true;
                neutralSlot._muteAudio = true;
                nhShuttleController._neutralSlot = neutralSlot;

                var orb = shuttleController._orb.gameObject;
                nhShuttleController._orb = orb.GetComponent<NomaiInterfaceOrb>();
                nhShuttleController._orb._sector = nhShuttleController._interiorSector;
                nhShuttleController._orb._slotRoot = slots;
                nhShuttleController._orb._safetyRails = slots.GetComponentsInChildren<OWRail>();
                nhShuttleController._orb._slots = slots.GetComponentsInChildren<NomaiInterfaceSlot>();
                _bodyPrefab = shuttleController._shuttleBody.gameObject?.InstantiateInactive()?.Rename("Prefab_QM_Shuttle_Body")?.DontDestroyOnLoad();
                nhShuttleController._shuttleBody = _bodyPrefab.GetComponent<OWRigidbody>();
                nhShuttleController._impactSensor = _bodyPrefab.GetComponent<ImpactSensor>();
                nhShuttleController._forceApplier = _bodyPrefab.GetComponentInChildren<ForceApplier>();
                nhShuttleController._detectorObj = nhShuttleController._forceApplier.gameObject;
                GameObject.DestroyImmediate(shuttleController);
                GameObject.DestroyImmediate(_prefab.FindChild("Sector_NomaiShuttleInterior/Interactibles_NomaiShuttleInterior/Prefab_NOM_Recorder"));
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, IModBehaviour mod, ShuttleInfo info)
        {
            InitPrefab();

            if (_prefab == null || planetGO == null || sector == null) return null;

            var detailInfo = new DetailInfo(info) { keepLoaded = true };
            var shuttleObject = DetailBuilder.Make(planetGO, sector, mod, _prefab, detailInfo);
            shuttleObject.SetActive(false);

            StreamingHandler.SetUpStreaming(shuttleObject, sector);

            NHShuttleController shuttleController = shuttleObject.GetComponent<NHShuttleController>();
            NomaiShuttleController.ShuttleID id = ShuttleHandler.GetShuttleID(info.id);
            shuttleController._id = id;
            shuttleController._cannon = Locator.GetGravityCannon(id);

            GameObject slots = shuttleObject.FindChild("Sector_NomaiShuttleInterior/Interactibles_NomaiShuttleInterior/ControlPanel/Slots");
            GameObject orbObject = shuttleController._orb.gameObject;
            orbObject.transform.SetParent(slots.transform, false);
            orbObject.transform.localPosition = new Vector3(-0.0153f, -0.2386f, 0.0205f);
            shuttleController._orb = orbObject.GetComponent<NomaiInterfaceOrb>();
            shuttleController._orb._sector = shuttleController._interiorSector;
            shuttleController._orb._slotRoot = slots;
            shuttleController._orb._safetyRails = slots.GetComponentsInChildren<OWRail>();
            shuttleController._orb._slots = slots.GetComponentsInChildren<NomaiInterfaceSlot>();

            StreamingHandler.SetUpStreaming(orbObject, sector);

            GameObject bodyObject = _bodyPrefab.InstantiateInactive().Rename("Shuttle_Body");
            bodyObject.transform.SetParent(shuttleObject.transform, false);
            bodyObject.transform.localPosition = Vector3.zero;
            bodyObject.transform.localEulerAngles = Vector3.zero;
            shuttleController._shuttleBody = bodyObject.GetComponent<OWRigidbody>();
            shuttleController._impactSensor = bodyObject.GetComponent<ImpactSensor>();
            shuttleController._forceApplier = bodyObject.GetComponentInChildren<ForceApplier>();
            shuttleController._detectorObj = shuttleController._forceApplier.gameObject;

            shuttleObject.SetActive(true);
            bodyObject.SetActive(true);
            orbObject.SetActive(true);
            shuttleController._orb.RemoveAllLocks();

            Shuttles[id] = shuttleController;

            return shuttleObject;
        }
    }
}
