using NewHorizons.Components;
using NewHorizons.Components.Achievement;
using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.Props
{
    public static class RaftBuilder
    {
        private static GameObject _prefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = GameObject.FindObjectOfType<RaftController>()?.gameObject?.InstantiateInactive()?.Rename("Raft_Body_Prefab")?.DontDestroyOnLoad();
                if (_prefab == null)
                {
                    Logger.LogWarning($"Tried to make a raft but couldn't. Do you have the DLC installed?");
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

        public static GameObject Make(GameObject planetGO, Sector sector, PropModule.RaftInfo info, OWRigidbody planetBody)
        {
            InitPrefab();

            if (_prefab == null || sector == null) return null;

            GameObject raftObject = _prefab.InstantiateInactive();
            raftObject.name = !string.IsNullOrEmpty(info.rename) ? info.rename : "Raft_Body";
            raftObject.transform.parent = sector?.transform ?? planetGO.transform;

            if (!string.IsNullOrEmpty(info.parentPath))
            {
                var newParent = planetGO.transform.Find(info.parentPath);
                if (newParent != null)
                {
                    raftObject.transform.parent = newParent;
                }
                else
                {
                    Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                }
            }

            var pos = (Vector3)(info.position ?? Vector3.zero);
            var rot = Quaternion.identity;
            if (info.isRelativeToParent)
            {
                raftObject.transform.localPosition = pos;
                raftObject.transform.localRotation = rot;
            }
            else
            {
                raftObject.transform.position = planetGO.transform.TransformPoint(pos);
                raftObject.transform.rotation = planetGO.transform.TransformRotation(rot);
            }

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
