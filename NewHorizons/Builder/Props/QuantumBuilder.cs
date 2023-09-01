using NewHorizons.Components.Quantum;
using NewHorizons.External.Modules.Props.Quantum;
using NewHorizons.Utility.Geometry;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// BUGS THAT REQUIRE REWRITING MOBIUS CODE
// 1) FIXED!                              - MultiStateQuantumObjects don't check to see if the new state would be visible before choosing it
// 2) FIXED? no longer supporting shuffle - QuantumShuffleObjects don't respect rotation, they set rotation to 0 on collapse
// 3)                                     - MultiStateQuantumObjects don't get locked by pictures

// New features to support
// 1) multiState._prerequisiteObjects
// 2) Socket groups that have an equal number of props and sockets
// 3) Nice to have: socket groups that specify a filledSocketObject and an emptySocketObject (eg the archway in the giant's deep tower)

namespace NewHorizons.Builder.Props
{
    public static class QuantumBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            switch (quantumGroup.type)
            {
                case QuantumGroupType.Sockets:
                    MakeSocketGroup(planetGO, sector, quantumGroup, propsInGroup);
                    return;
                case QuantumGroupType.States:
                    MakeStateGroup(planetGO, sector, quantumGroup, propsInGroup);
                    return;
                    /*
                    case PropModule.QuantumGroupType.Shuffle: 
                        MakeShuffleGroup(go, sector, quantumGroup, propsInGroup); 
                        return;
                    */
            }
        }

        /*
        public static void MakeQuantumLightning(GameObject planetGO, Sector sector, GameObject[] propsInGroup)
        {
            var lightning = DetailBuilder.Make(planetGO, sector, AssetBundleUtilities.EyeLightning.LoadAsset<GameObject>("Prefab_EYE_QuantumLightningObject"), new DetailInfo());
            lightning.transform.position = prop.transform.position;
            lightning.transform.rotation = prop.transform.rotation;
            lightning.transform.parent = prop.transform.parent;

            var lightningStatesParent = lightning.transform.Find("Models");
            prop.transform.parent = lightningStatesParent;

            var lightningController = lightning.GetComponent<QuantumLightningObject>();
            lightningController._models = new GameObject[] { prop };
            lightningController.enabled = true;
        }
        */

        public static void MakeSocketGroup(GameObject planetGO, Sector sector, QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            var groupRoot = new GameObject("Quantum Sockets - " + quantumGroup.id);
            groupRoot.transform.parent = sector?.transform ?? planetGO.transform;
            groupRoot.transform.localPosition = Vector3.zero;
            groupRoot.transform.localEulerAngles = Vector3.zero;

            var sockets = new QuantumSocket[quantumGroup.sockets.Length];
            for (int i = 0; i < quantumGroup.sockets.Length; i++)
            {
                var socketInfo = quantumGroup.sockets[i];

                var socketGO = GeneralPropBuilder.MakeNew("Socket " + i, planetGO, sector, socketInfo, parentOverride: groupRoot.transform);
                var socket = socketGO.AddComponent<QuantumSocket>();
                socket._lightSources = new Light[0];
                sockets[i] = socket;

                socketGO.SetActive(true);
            }

            foreach (var prop in propsInGroup)
            {
                prop.SetActive(false);

                var quantumObject = prop.AddComponent<SocketedQuantumObject>();
                quantumObject._socketRoot = groupRoot;
                quantumObject._socketList = sockets.ToList();
                quantumObject._sockets = sockets;
                quantumObject._prebuilt = true;
                quantumObject._childSockets = new List<QuantumSocket>();
                // TODO: support _alignWithGravity?

                if (prop.GetComponentInChildren<VisibilityTracker>() == null)
                {
                    BoundsUtilities.AddBoundsVisibility(prop);
                }

                prop.SetActive(true);
            }


        }

        public static void MakeStateGroup(GameObject go, Sector sector, QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            var groupRoot = new GameObject("Quantum States - " + quantumGroup.id);
            groupRoot.transform.parent = sector?.transform ?? go.transform;
            groupRoot.transform.localPosition = Vector3.zero;

            var states = new List<QuantumState>();
            foreach (var prop in propsInGroup)
            {
                prop.transform.parent = groupRoot.transform;
                var state = prop.AddComponent<QuantumState>();
                state._probability = 1;
                states.Add(state);

                if (prop.GetComponentInChildren<ShapeVisibilityTracker>() != null) continue;

                BoundsUtilities.AddBoundsVisibility(prop);
            }

            if (quantumGroup.hasEmptyState)
            {
                var template = propsInGroup[0];

                var empty = new GameObject("Empty State");
                empty.transform.parent = groupRoot.transform;
                var state = empty.AddComponent<QuantumState>();
                states.Add(state);

                var boxBounds = BoundsUtilities.GetBoundsOfSelfAndChildMeshes(template);
                var boxShape = empty.AddComponent<BoxShape>();
                boxShape.center = boxBounds.center;
                boxShape.extents = boxBounds.size;
                if (Main.Debug) empty.AddComponent<BoxShapeVisualizer>();

                empty.AddComponent<ShapeVisibilityTracker>();
            }

            groupRoot.SetActive(false);
            var multiState = groupRoot.AddComponent<NHMultiStateQuantumObject>();
            multiState._loop = quantumGroup.loop;
            multiState._sequential = quantumGroup.sequential;
            multiState._states = states.ToArray();
            multiState._prerequisiteObjects = new MultiStateQuantumObject[0]; // TODO: support this
            multiState._initialState = 0;
            groupRoot.SetActive(true);
        }

        public static void MakeShuffleGroup(GameObject go, Sector sector, QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            //var averagePosition = propsInGroup.Aggregate(Vector3.zero, (avg, prop) => avg + prop.transform.position) / propsInGroup.Count();
            GameObject shuffleParent = new GameObject("Quantum Shuffle - " + quantumGroup.id);
            shuffleParent.SetActive(false);
            shuffleParent.transform.parent = sector?.transform ?? go.transform;
            shuffleParent.transform.localPosition = Vector3.zero;
            propsInGroup.ToList().ForEach(p => p.transform.parent = shuffleParent.transform);

            var shuffle = shuffleParent.AddComponent<QuantumShuffleObject>();
            shuffle._shuffledObjects = propsInGroup.Select(p => p.transform).ToArray();
            shuffle.Awake(); // this doesn't get called on its own for some reason

            BoundsUtilities.AddBoundsVisibility(shuffleParent);
            shuffleParent.SetActive(true);
        }
    }
}
