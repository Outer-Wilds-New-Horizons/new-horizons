using NewHorizons.Components.Quantum;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.Quantum;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.Geometry;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class QuantumBuilder
    {
        public static void TryMake(GameObject go, Sector sector, PlanetConfig config)
        {
            if (config.Props != null && config.Props.details != null)
            {
                var quantumPropsByGroup = new Dictionary<string, List<(GameObject go, DetailInfo info)>>();
                foreach (var detail in config.Props?.details)
                {
                    if (detail.quantumGroupID != null)
                    {
                        if (!quantumPropsByGroup.ContainsKey(detail.quantumGroupID))
                        {
                            quantumPropsByGroup[detail.quantumGroupID] = new();
                        }
                        quantumPropsByGroup[detail.quantumGroupID].Add((DetailBuilder.GetSpawnedGameObjectByDetailInfo(detail), detail));
                    }
                }

                void MakeQuantumGroup(BaseQuantumGroupInfo[] group)
                {
                    foreach (var quantumGroup in group)
                    {
                        if (!quantumPropsByGroup.ContainsKey(quantumGroup.id)) continue;
                        var propsInGroup = quantumPropsByGroup[quantumGroup.id];

                        try
                        {
                            QuantumBuilder.Make(go, sector, quantumGroup, propsInGroup.ToArray());
                        }
                        catch (Exception ex)
                        {
                            NHLogger.LogError($"Couldn't make quantum group [{quantumGroup.id}] for [{go.name}]:\n{ex}");
                        }
                    }
                }

                if (config.Props.socketQuantumGroups != null)
                {
                    MakeQuantumGroup(config.Props.socketQuantumGroups);
                }

                if (config.Props.stateQuantumGroups != null)
                {
                    MakeQuantumGroup(config.Props.stateQuantumGroups);
                }

                if (config.Props.lightningQuantumGroups != null)
                {
                    MakeQuantumGroup(config.Props.lightningQuantumGroups);
                }
            }
        }

        public static void Make(GameObject planetGO, Sector sector, BaseQuantumGroupInfo quantumGroup, (GameObject go, DetailInfo detail)[] propsInGroup)
        {
            if (quantumGroup is SocketQuantumGroupInfo socketGroup)
            {
                MakeSocketGroup(planetGO, sector, socketGroup, propsInGroup.Select(x => x.go).ToArray());
            }
            else if (quantumGroup is StateQuantumGroupInfo stateGroup)
            {
                MakeStateGroup(planetGO, sector, stateGroup, propsInGroup.Select(x => x.go).ToArray());
            }
            else if (quantumGroup is LightningQuantumGroupInfo lightningGroup)
            {
                MakeQuantumLightning(planetGO, sector, lightningGroup, propsInGroup);
            }
        }

        public static void MakeQuantumLightning(GameObject planetGO, Sector sector, LightningQuantumGroupInfo quantumGroup, (GameObject go, DetailInfo detail)[] propsInGroup)
        {
            // Bases its position off the first object with position set
            var root = propsInGroup.FirstOrDefault(x => x.detail.position != null || x.detail.rotation != null || x.detail.isRelativeToParent);

            var rootGo = root == default ? sector.gameObject : root.go;

            var lightning = DetailBuilder.Make(planetGO, sector, Main.Instance, AssetBundleUtilities.EyeLightning.LoadAsset<GameObject>("Prefab_EYE_QuantumLightningObject"), new DetailInfo());
            lightning.transform.position = rootGo.transform.position;
            lightning.transform.rotation = rootGo.transform.rotation;
            lightning.transform.parent = rootGo.transform.parent;

            var lightningStatesParent = lightning.transform.Find("Models");
            foreach (var (go, _) in propsInGroup)
            {
                go.transform.parent = lightningStatesParent;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
            }

            var lightningController = lightning.GetComponent<QuantumLightningObject>();
            lightningController._models = propsInGroup.Select(x => x.go).ToArray();
            lightningController.enabled = true;

            lightning.name = "Quantum Lightning - " + quantumGroup.id;
            lightning.SetActive(true);
        }

        // TODO: Socket groups that have an equal number of props and sockets
        // Nice to have: socket groups that specify a filledSocketObject and an emptySocketObject (eg the archway in the giant's deep tower)
        public static void MakeSocketGroup(GameObject planetGO, Sector sector, SocketQuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            GameObject specialProp = null;
            if (propsInGroup.Length == quantumGroup.sockets.Length)
            {
                // Special case!
                specialProp = propsInGroup.Last();
                var propsInGroupList = propsInGroup.ToList();
                propsInGroupList.RemoveAt(propsInGroup.Length - 1);
                propsInGroup = propsInGroupList.ToArray();
            }

            var groupRoot = new GameObject("Quantum Sockets - " + quantumGroup.id);
            groupRoot.transform.parent = sector?.transform ?? planetGO.transform;
            groupRoot.transform.localPosition = Vector3.zero;
            groupRoot.transform.localEulerAngles = Vector3.zero;

            var sockets = new QuantumSocket[quantumGroup.sockets.Length];
            for (int i = 0; i < quantumGroup.sockets.Length; i++)
            {
                var socketInfo = quantumGroup.sockets[i];

                var socket = GeneralPropBuilder.MakeNew("Socket " + i, planetGO, sector, socketInfo, defaultParent: groupRoot.transform);

                sockets[i] = socket.AddComponent<QuantumSocket>();
                sockets[i]._lightSources = new Light[0]; // TODO: make this customizable?
                socket.SetActive(true);
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

            if (specialProp != null)
            {
                // Can't have 4 objects in 4 slots
                // Instead we have a duplicate of the final object for each slot, which appears when that slot is "empty"
                for (int i = 0; i < sockets.Length; i++)
                {
                    var emptySocketObject = DetailBuilder.Make(planetGO, sector, Main.Instance, specialProp, new DetailInfo());
                    var socket = sockets[i];
                    socket._emptySocketObject = emptySocketObject;
                    emptySocketObject.SetActive(socket._quantumObject == null);
                    emptySocketObject.transform.parent = socket.transform;
                    emptySocketObject.transform.localPosition = Vector3.zero;
                    emptySocketObject.transform.localRotation = Quaternion.identity;

                    // Need to add a visibility tracker for this socket else it doesn't stay "empty" when photographed
                    socket.SetActive(false);
                    var tracker = new GameObject("VisibilityTracker");
                    tracker.transform.parent = socket.transform;
                    tracker.transform.localPosition = Vector3.zero;
                    tracker.transform.localRotation = Quaternion.identity;
                    var box = tracker.AddComponent<BoxShape>();
                    box.size = new Vector3(0.2f, 0.6f, 0.2f);
                    box.center = new Vector3(0, 0.3f, 0);
                    tracker.AddComponent<ShapeVisibilityTracker>();
                    // Using a quantum object bc it can be locked by camera
                    socket._visibilityObject = socket.gameObject.AddComponent<SnapshotLockableVisibilityObject>();
                    socket.SetActive(true);
                }
            }
        }

        public static void MakeStateGroup(GameObject go, Sector sector, StateQuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            // NOTE: States groups need special consideration that socket groups don't
            // this is because the base class QuantumObject (and this is important) IGNORES PICTURES TAKEN FROM OVER 100 METERS AWAY
            // why does this affect states and not sockets? Well because sockets put the QuantumObject component (QuantumSocketedObject) on the actual props themselves
            // while states put the QuantumObject component (NHMultiStateQuantumObject) on the parent, which is located at the center of the planet
            // this means that the distance measured by QuantumObject is not accurate, since it's not measuring from the active prop, but from the center of the planet

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
                empty.AddComponent<BoxShapeVisualizer>();
                empty.AddComponent<ShapeVisibilityTracker>();
            }

            groupRoot.SetActive(false);
            var multiState = groupRoot.AddComponent<NHMultiStateQuantumObject>();
            multiState._loop = quantumGroup.loop;
            multiState._sequential = quantumGroup.sequential;
            multiState._states = states.ToArray();
            multiState._prerequisiteObjects = new MultiStateQuantumObject[0]; // TODO: _prerequisiteObjects
            multiState._initialState = 0;
            // snapshot events arent listened to outside of the sector, so fortunately this isnt really infinite 
            multiState._maxSnapshotLockRange = Mathf.Infinity; // TODO: maybe expose this at some point if it breaks a puzzle or something
            groupRoot.SetActive(true);
        }

        public static void MakeShuffleGroup(GameObject go, Sector sector, BaseQuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            //var averagePosition = propsInGroup.Aggregate(Vector3.zero, (avg, prop) => avg + prop.transform.position) / propsInGroup.Count();
            GameObject shuffleParent = new GameObject("Quantum Shuffle - " + quantumGroup.id);
            shuffleParent.SetActive(false);
            shuffleParent.transform.parent = sector?.transform ?? go.transform;
            shuffleParent.transform.localPosition = Vector3.zero;
            propsInGroup.ToList().ForEach(p => p.transform.parent = shuffleParent.transform);

            var shuffle = shuffleParent.AddComponent<QuantumShuffleObject>();
            shuffle._shuffledObjects = propsInGroup.Select(p => p.transform).ToArray();
            shuffle.Awake(); // this doesn't get called on its own for some reason. what? how?

            AddBoundsVisibility(shuffleParent);
            shuffleParent.SetActive(true);
        }


        struct BoxShapeReciever
        {
            public MeshFilter f;
            public SkinnedMeshRenderer s;
            public GameObject gameObject;
        }

        public static void AddBoundsVisibility(GameObject g)
        {
            var meshFilters = g.GetComponentsInChildren<MeshFilter>();
            var skinnedMeshRenderers = g.GetComponentsInChildren<SkinnedMeshRenderer>();

            var boxShapeRecievers = meshFilters
                .Select(f => new BoxShapeReciever() { f = f, gameObject = f.gameObject })
                .Concat(
                    skinnedMeshRenderers.Select(s => new BoxShapeReciever() { s = s, gameObject = s.gameObject })
                )
                .ToList();

            foreach (var boxshapeReciever in boxShapeRecievers)
            {
                var box = boxshapeReciever.gameObject.AddComponent<BoxShape>();
                boxshapeReciever.gameObject.AddComponent<ShapeVisibilityTracker>();
                boxshapeReciever.gameObject.AddComponent<BoxShapeVisualizer>();

                var fixer = boxshapeReciever.gameObject.AddComponent<BoxShapeFixer>();
                fixer.shape = box;
                fixer.meshFilter = boxshapeReciever.f;
                fixer.skinnedMeshRenderer = boxshapeReciever.s;
            }
        }

        // BUG: ignores skinned guys. this coincidentally makes it work without BoxShapeFixer
        public static Bounds GetBoundsOfSelfAndChildMeshes(GameObject g)
        {
            var meshFilters = g.GetComponentsInChildren<MeshFilter>();
            var corners = meshFilters.SelectMany(m => GetMeshCorners(m, g)).ToList();

            Bounds b = new Bounds(corners[0], Vector3.zero);
            corners.ForEach(corner => b.Encapsulate(corner));

            return b;
        }

        public static Vector3[] GetMeshCorners(MeshFilter m, GameObject relativeTo = null)
        {
            var bounds = m.mesh.bounds;

            var localCorners = new Vector3[]
            {
                 bounds.min,
                 bounds.max,
                 new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                 new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                 new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                 new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
                 new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                 new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
            };

            var globalCorners = localCorners.Select(localCorner => m.transform.TransformPoint(localCorner)).ToArray();

            if (relativeTo == null) return globalCorners;

            return globalCorners.Select(globalCorner => relativeTo.transform.InverseTransformPoint(globalCorner)).ToArray();
        }
    }

    /// <summary>
    /// for some reason mesh bounds are wrong unless we wait a bit
    /// so this script contiously checks everything until it is correct
    ///
    /// this actually only seems to be a problem with skinned renderers. normal ones work fine
    /// TODO: at some point narrow this down to just skinned, instead of doing everything and checking every frame
    /// </summary>
    public class BoxShapeFixer : MonoBehaviour
    {
        public BoxShape shape;
        public MeshFilter meshFilter;
        public SkinnedMeshRenderer skinnedMeshRenderer;

        public void Update()
        {
            if (meshFilter == null && skinnedMeshRenderer == null)
            {
                NHLogger.LogVerbose("Useless BoxShapeFixer, destroying");
                DestroyImmediate(this);
            }

            Mesh sharedMesh = null;
            if (meshFilter != null) sharedMesh = meshFilter.sharedMesh;
            if (skinnedMeshRenderer != null) sharedMesh = skinnedMeshRenderer.sharedMesh;

            if (sharedMesh == null) return;
            if (sharedMesh.bounds.size == Vector3.zero) return;

            shape.size = sharedMesh.bounds.size;
            shape.center = sharedMesh.bounds.center;

            DestroyImmediate(this);
        }
    }
}
