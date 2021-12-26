using NewHorizons.Builder.General;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class RaftBuilder
    {
        private static GameObject originalRaft;

        public static void Make(GameObject body, Vector3 position, Sector sector, OWRigidbody OWRB, AstroObject ao)
        {
            if(originalRaft == null) originalRaft = GameObject.Instantiate(GameObject.Find("RingWorld_Body/Sector_RingInterior/Interactibles_RingInterior/Rafts/Raft_Body"));

            GameObject raftObject = new GameObject($"{body.name}Raft");

            GameObject lightSensors = GameObject.Instantiate(GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone2/Structures_Zone2/RaftHouse_Eye_Zone2/Interactables_RaftHouse_Eye_Zone2/Prefab_IP_RaftDock/RaftSocket/Raft_Body (7)/LightSensorRoot"), raftObject.transform);
            GameObject geometry = GameObject.Instantiate(GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone2/Structures_Zone2/RaftHouse_Eye_Zone2/Interactables_RaftHouse_Eye_Zone2/Prefab_IP_RaftDock/RaftSocket/Raft_Body (7)/Structure_IP_Raft"), raftObject.transform);
            GameObject colliders = GameObject.Instantiate(GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone2/Structures_Zone2/RaftHouse_Eye_Zone2/Interactables_RaftHouse_Eye_Zone2/Prefab_IP_RaftDock/RaftSocket/Raft_Body (7)/Colliders/"), raftObject.transform);

            raftObject.transform.parent = sector.transform;
            raftObject.transform.localPosition = position;
            raftObject.transform.rotation = Quaternion.FromToRotation(raftObject.transform.TransformDirection(Vector3.up), position.normalized);

            foreach (var l in lightSensors.GetComponentsInChildren<SingleLightSensor>())
            {
                l.SetValue("_sector", sector);
                l.OnDetectLight += () => Logger.Log("LIGHT!!!");
                l.SetValue("_lightSourceMask", LightSourceType.FLASHLIGHT);
                l.SetDetectorActive(true);
                l.gameObject.SetActive(true);
                Logger.Log($"{l}");
            }

            foreach (var child in raftObject.GetComponentsInChildren<StreamingRenderMeshHandle>())
            {
                StreamingManager.LoadStreamingAssets(child.assetBundle);
            }

            var a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            a.transform.parent = raftObject.transform;
            a.transform.localPosition = Vector3.zero;

            //raftObject.AddComponent<PlanetaryRaftController>();

            /*
            var raftRB = raftObject.AddComponent<OWRigidbody>();
            raftObject.AddComponent<Rigidbody>();
            raftObject.AddComponent<KinematicRigidbody>();
            raftRB.SetVelocity(OWRB.GetVelocity());

            var motion = raftObject.AddComponent<MatchInitialMotion>();
            motion.SetBodyToMatch(OWRB);

            DetectorBuilder.Make(raftObject, raftRB, ao, null, false);

           var targetBodyAlignment = raftObject.AddComponent<AlignWithTargetBody>();
           targetBodyAlignment.SetTargetBody(OWRB);
           targetBodyAlignment.SetUsePhysicsToRotate(true);

            raftObject.GetComponent<CenterOfTheUniverseOffsetApplier>().Init(raftRB);
            */

            raftObject.SetActive(true);
        }
    }
}
