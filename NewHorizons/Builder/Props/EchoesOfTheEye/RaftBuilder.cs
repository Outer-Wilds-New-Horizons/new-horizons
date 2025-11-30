using NewHorizons.Components.Props;
using NewHorizons.External.Modules.Props.EchoesOfTheEye;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Builder.Props.EchoesOfTheEye
{
    public static class RaftBuilder
    {
        private static GameObject _prefab;
        private static GameObject _cleanPrefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = Object.FindObjectOfType<RaftController>()?.gameObject?.InstantiateInactive()?.Rename("Raft_Body_Prefab")?.DontDestroyOnLoad();
                if (_prefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a raft but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _prefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                    var raftController = _prefab.GetComponent<RaftController>();
                    if (raftController._sector != null)
                    {
                        // Since awake already ran we have to unhook these events
                        raftController._sector.OnOccupantEnterSector -= raftController.OnOccupantEnterSector;
                        raftController._sector.OnOccupantExitSector -= raftController.OnOccupantExitSector;
                        raftController._sector = null;
                    }
                    raftController._riverFluid = null;
                    foreach (var lightSensor in _prefab.GetComponentsInChildren<SingleLightSensor>())
                    {
                        if (lightSensor._sector != null)
                        {
                            lightSensor._sector.OnSectorOccupantsUpdated -= lightSensor.OnSectorOccupantsUpdated;
                            lightSensor._sector = null;
                        }
                        lightSensor._detectDreamLanterns = true;
                        lightSensor._lanternFocusThreshold = 0.9f;
                        lightSensor._illuminatingDreamLanternList = new List<DreamLanternController>();
                        lightSensor._lightSourceMask |= LightSourceType.DREAM_LANTERN;
                    }

                    // TODO: Change to one mesh
                    var twRaftRoot = new GameObject("Effects_IP_SIM_Raft");
                    twRaftRoot.transform.SetParent(_prefab.transform, false);
                    var twRaft = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Interactibles_Dreamworld/DreamRaft_Body/Effects_IP_SIM_Raft_1Way")
                        .Instantiate(Vector3.zero, Quaternion.identity, twRaftRoot.transform).Rename("Effects_IP_SIM_Raft_1Way");
                    twRaft.Instantiate(Vector3.zero, Quaternion.Euler(0, 180, 0), twRaftRoot.transform).Rename(twRaft.name);
                    twRaft.Instantiate(Vector3.zero, Quaternion.Euler(0, 90, 0), twRaftRoot.transform).Rename(twRaft.name);
                    twRaft.Instantiate(Vector3.zero, Quaternion.Euler(0, -90, 0), twRaftRoot.transform).Rename(twRaft.name);
                }
            }
            if (_cleanPrefab == null && _prefab != null)
            {
                _cleanPrefab = _prefab?.InstantiateInactive()?.Rename("Raft_Body_Prefab_Clean")?.DontDestroyOnLoad();
                if (_cleanPrefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a raft but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    var raftController = _cleanPrefab.GetComponent<RaftController>();
                    var rwRaft = _cleanPrefab.FindChild("Structure_IP_Raft");
                    var rwRaftAnimator = rwRaft.GetComponent<Animator>();
                    var rac = rwRaftAnimator.runtimeAnimatorController;
                    Object.DestroyImmediate(rwRaft);

                    var dwRaft = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Interactibles_Dreamworld/DreamRaft_Body/Structure_IP_Raft")
                        .Instantiate(Vector3.zero, Quaternion.identity, _cleanPrefab.transform).Rename("Structure_IP_DreamRaft");
                    dwRaft.transform.SetSiblingIndex(3);
                    foreach (var child in dwRaft.GetAllChildren())
                    {
                        child.SetActive(true);
                    }

                    var dwRaftAnimator = dwRaft.AddComponent<Animator>();
                    dwRaftAnimator.runtimeAnimatorController = rac;
                    raftController._railingAnimator = dwRaftAnimator;

                    var dwLightSensorForward = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Interactibles_Dreamworld/DreamRaft_Body/LightSensor_Forward");
                    var dwLightSensorOrigMaterial = dwLightSensorForward.GetComponent<LightSensorEffects>()._origMaterial;
                    var dwLightSensor = dwLightSensorForward.FindChild("Structure_IP_Raft_Sensor");
                    ChangeSensor(_cleanPrefab.FindChild("LightSensorRoot/LightSensor_Forward"), dwLightSensorOrigMaterial, dwLightSensor);
                    ChangeSensor(_cleanPrefab.FindChild("LightSensorRoot/LightSensor_Right"), dwLightSensorOrigMaterial, dwLightSensor);
                    ChangeSensor(_cleanPrefab.FindChild("LightSensorRoot/LightSensor_Rear"), dwLightSensorOrigMaterial, dwLightSensor, true);
                    ChangeSensor(_cleanPrefab.FindChild("LightSensorRoot/LightSensor_Left"), dwLightSensorOrigMaterial, dwLightSensor);
                }
            }
        }

        private static void ChangeSensor(GameObject lightSensor, Material origMaterial, GameObject newLightSensor, bool reverse = false)
        {
            var singleLightSensor = lightSensor.GetComponent<SingleLightSensor>();
            var lightSensorEffects = lightSensor.GetComponent<LightSensorEffects>();
            lightSensorEffects._lightSensor = singleLightSensor;
            Object.DestroyImmediate(lightSensor.FindChild("Structure_IP_Raft_Sensor"));
            lightSensorEffects._origMaterial = origMaterial;
            var copiedLightSensor = newLightSensor
                .Instantiate(reverse ? new Vector3(0, -1.5f, -0.1303f) : new Vector3(0, -1.5f, -0.0297f),
                             reverse ? Quaternion.identity : Quaternion.Euler(0, 180, 0),
                             lightSensor.transform).Rename("Structure_IP_DreamRaft_Sensor");
            var bulb = copiedLightSensor.FindChild("Props_IP_Raft_Lamp_geoBulb");
            lightSensorEffects._renderer = bulb.GetComponent<MeshRenderer>();
            lightSensorEffects._lightRenderer = bulb.GetComponent<OWRenderer>();
        }

        public static GameObject Make(GameObject planetGO, Sector sector, RaftInfo info, OWRigidbody planetBody)
        {
            InitPrefab();

            if (_prefab == null || _cleanPrefab == null || sector == null) return null;

            GameObject raftObject = GeneralPropBuilder.MakeFromPrefab(info.pristine ? _cleanPrefab : _prefab, "Raft_Body", planetGO, ref sector, info);

            StreamingHandler.SetUpStreaming(raftObject, sector);

            var raftController = raftObject.GetComponent<RaftController>();
            raftController._sector = sector;
            raftController._acceleration = info.acceleration;
            sector.OnOccupantEnterSector += raftController.OnOccupantEnterSector;
            sector.OnOccupantExitSector += raftController.OnOccupantExitSector;

            // Detectors
            var fluidDetector = raftObject.transform.Find("Detector_Raft").GetComponent<RaftFluidDetector>();
            var waterVolume = planetGO.GetComponentInChildren<RadialFluidVolume>();
            fluidDetector._alignmentFluid = waterVolume;
            fluidDetector._buoyancy.checkAgainstWaves = true;
            // Rafts were unable to trigger docks because these were disabled for some reason
            fluidDetector.GetComponent<BoxShape>().enabled = true;
            fluidDetector.GetComponent<OWCollider>().enabled = true;

            // Light sensors
            foreach (var lightSensor in raftObject.GetComponentsInChildren<SingleLightSensor>())
            {
                lightSensor._sector = sector;
                sector.OnSectorOccupantsUpdated += lightSensor.OnSectorOccupantsUpdated;
            }

            var nhRaftController = raftObject.AddComponent<NHRaftController>();

            var achievementObject = new GameObject("AchievementVolume");
            achievementObject.transform.SetParent(raftObject.transform, false);

            var shape = achievementObject.AddComponent<SphereShape>();
            shape.radius = 3;
            shape.SetCollisionMode(Shape.CollisionMode.Volume);

            achievementObject.AddComponent<OWTriggerVolume>()._shape = shape;
            achievementObject.AddComponent<OtherMods.AchievementsPlus.NH.RaftingAchievement>();

            raftObject.SetActive(true);

            if (planetGO != null && !string.IsNullOrEmpty(info.dockPath))
            {
                var dockTransform = planetGO.transform.Find(info.dockPath);
                if (dockTransform != null && dockTransform.TryGetComponent(out RaftDock raftDock))
                {
                    raftController.SkipSuspendOnStart();
                    raftDock._startRaft = raftController;
                    raftDock._raft = raftController;
                }
                else
                {
                    NHLogger.LogError($"Cannot find raft dock object at path: {planetGO.name}/{info.dockPath}");
                }
            }

            return raftObject;
        }
    }
}
