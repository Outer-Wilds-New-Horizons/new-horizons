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

        public static void Make(GameObject planetGO, Sector sector, PropModule.RaftInfo info, OWRigidbody planetBody)
        {
            if (_prefab == null)
            {
                _prefab = GameObject.FindObjectOfType<RaftController>()?.gameObject?.InstantiateInactive();
                if (_prefab == null)
                {
                    Logger.LogWarning($"Tried to make a raft but couldn't. Do you have the DLC installed?");
                    return;
                }
                _prefab.name = "Raft_Body_Prefab";
            }

            GameObject raftObject = _prefab.InstantiateInactive();
            raftObject.name = "Raft_Body";
            raftObject.transform.parent = sector?.transform ?? planetGO.transform;
            raftObject.transform.position = planetGO.transform.TransformPoint(info.position);
            raftObject.transform.rotation = planetGO.transform.TransformRotation(Quaternion.identity);

            StreamingHandler.SetUpStreaming(raftObject, sector);

            var raftController = raftObject.GetComponent<RaftController>();
            // Since awake already ran we have to unhook these events
            raftController._sector.OnOccupantEnterSector -= raftController.OnOccupantEnterSector;
            raftController._sector.OnOccupantExitSector -= raftController.OnOccupantExitSector;
            raftController._riverFluid = null;

            raftController._sector = sector;
            sector.OnOccupantEnterSector += raftController.OnOccupantEnterSector;
            sector.OnOccupantExitSector += raftController.OnOccupantExitSector;

            // Detectors
            var fluidDetector = raftObject.transform.Find("Detector_Raft").GetComponent<RaftFluidDetector>();
            var waterVolume = planetGO.GetComponentInChildren<NHFluidVolume>();
            fluidDetector._alignmentFluid = waterVolume;

            // Light sensors
            foreach (var lightSensor in raftObject.GetComponentsInChildren<SingleLightSensor>())
            {
                lightSensor._sector.OnSectorOccupantsUpdated -= lightSensor.OnSectorOccupantsUpdated;
                lightSensor._sector = sector;
                sector.OnSectorOccupantsUpdated += lightSensor.OnSectorOccupantsUpdated;
            }

            raftObject.SetActive(true);
        }
    }
}
