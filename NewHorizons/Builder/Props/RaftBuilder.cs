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
        public static void Make(GameObject body, Vector3 position, Sector sector, OWRigidbody OWRB, AstroObject ao)
        {
            var originalRaft = GameObject.Find("RingWorld_Body/Sector_RingInterior/Interactibles_RingInterior/Rafts/Raft_Body");

            GameObject raftObject = new GameObject($"{body.name}Raft");
            raftObject.SetActive(false);

            GameObject lightSensors = GameObject.Instantiate(GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone2/Structures_Zone2/RaftHouse_Eye_Zone2/Interactables_RaftHouse_Eye_Zone2/Prefab_IP_RaftDock/RaftSocket/Raft_Body (7)/LightSensorRoot"), raftObject.transform);
            GameObject geometry = GameObject.Instantiate(GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone2/Structures_Zone2/RaftHouse_Eye_Zone2/Interactables_RaftHouse_Eye_Zone2/Prefab_IP_RaftDock/RaftSocket/Raft_Body (7)/Structure_IP_Raft"), raftObject.transform);
            GameObject collidersObject = GameObject.Instantiate(GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone2/Structures_Zone2/RaftHouse_Eye_Zone2/Interactables_RaftHouse_Eye_Zone2/Prefab_IP_RaftDock/RaftSocket/Raft_Body (7)/Colliders/"), raftObject.transform);

            raftObject.transform.parent = sector.transform;
            raftObject.transform.localPosition = position;
            raftObject.transform.rotation = Quaternion.FromToRotation(raftObject.transform.TransformDirection(Vector3.up), position.normalized);

            foreach (var l in lightSensors.GetComponentsInChildren<SingleLightSensor>())
            {
                if (l._sector != null) l._sector.OnSectorOccupantsUpdated -= l.OnSectorOccupantsUpdated;
                l._sector = sector;
                l._sector.OnSectorOccupantsUpdated += l.OnSectorOccupantsUpdated;
                l._lightSourceMask = LightSourceType.FLASHLIGHT;
                l.SetDetectorActive(true);
                l.gameObject.SetActive(true);
            }

            var rigidBody = raftObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.interpolation = originalRaft.GetComponent<Rigidbody>().interpolation;
            rigidBody.collisionDetectionMode = originalRaft.GetComponent<Rigidbody>().collisionDetectionMode;
            rigidBody.mass = originalRaft.GetComponent<Rigidbody>().mass;
            rigidBody.drag = originalRaft.GetComponent<Rigidbody>().drag;
            rigidBody.angularDrag = originalRaft.GetComponent<Rigidbody>().angularDrag;

            var kinematicRigidBody = raftObject.AddComponent<KinematicRigidbody>();
            kinematicRigidBody.centerOfMass = Vector3.zero;

            var owRigidBody = raftObject.AddComponent<OWRigidbody>();
            owRigidBody.SetValue("_kinematicSimulation", true);
            owRigidBody.SetValue("_rigidbody", rigidBody);
            owRigidBody.SetValue("_kinematicRigidbody", kinematicRigidBody);
            kinematicRigidBody._rigidbody = rigidBody;
            kinematicRigidBody._owRigidbody = owRigidBody;

            //var motion = raftObject.AddComponent<MatchInitialMotion>();
            //motion.SetBodyToMatch(OWRB);
            
            foreach (var c in collidersObject.GetComponentsInChildren<OWCollider>())
            {
                c.ClearParentBody();
                c._parentBody = owRigidBody;
                c.ListenForParentBodySuspension();
            }
            var meshColliders = collidersObject.GetComponentsInChildren<MeshCollider>();
            foreach (var c in meshColliders)
            {
                var child = c.gameObject;
                var newCollider = child.AddComponent<MeshCollider>();
                newCollider.sharedMesh = c.sharedMesh;
                newCollider.material = c.material;
                newCollider.sharedMaterial = c.sharedMaterial;
                GameObject.Destroy(c);
            }

            foreach (var child in raftObject.GetComponentsInChildren<StreamingRenderMeshHandle>())
            {
                StreamingManager.LoadStreamingAssets(child.assetBundle);
            }

            var detectorGO = DetectorBuilder.Make(raftObject, owRigidBody, ao, null, false, false);
            var fluidDetector = detectorGO.AddComponent<DynamicFluidDetector>();
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => fluidDetector._activeVolumes = new EffectVolume[] { body.GetComponentInChildren<RadialFluidVolume>() }.ToList());


            var targetBodyAlignment = raftObject.AddComponent<AlignWithTargetBody>();
            targetBodyAlignment._owRigidbody = owRigidBody;
            targetBodyAlignment.SetTargetBody(OWRB);
            targetBodyAlignment.SetUsePhysicsToRotate(true);

            var controller = raftObject.AddComponent<PlanetaryRaftController>();
            controller.SetSector(sector);

            raftObject.SetActive(true);
        }
    }
}
