using NewHorizons.Components.Props;
using NewHorizons.External.Modules.Props.EchoesOfTheEye;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class RaftBuilder
    {
        private static GameObject _prefab;

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
                    }
                }
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, RaftInfo info, OWRigidbody planetBody)
        {
            InitPrefab();

            if (_prefab == null || sector == null) return null;

            GameObject raftObject = GeneralPropBuilder.MakeFromPrefab(_prefab, "Raft_Body", planetGO, sector, info);

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

            return raftObject;
        }
    }
}
