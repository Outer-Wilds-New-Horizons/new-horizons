using NewHorizons.Components.Quantum;
using NewHorizons.External.Modules.Props.Quantum;
using NewHorizons.Utility.Geometry;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        
        // TODO: Socket groups that have an equal number of props and sockets
        // Nice to have: socket groups that specify a filledSocketObject and an emptySocketObject (eg the archway in the giant's deep tower)
        public static void MakeSocketGroup(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod, QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            var groupRoot = new GameObject("Quantum Sockets - " + quantumGroup.id);
            groupRoot.transform.parent = sector?.transform ?? planetGO.transform;
            groupRoot.transform.localPosition = Vector3.zero;
            groupRoot.transform.localEulerAngles = Vector3.zero;

            var sockets = new QuantumSocket[quantumGroup.sockets.Length];
            for (int i = 0; i < quantumGroup.sockets.Length; i++)
            {
                var socketInfo = quantumGroup.sockets[i];

                var socket = GeneralPropBuilder.MakeNew("Socket " + i, go, sector, socketInfo, defaultParent: groupRoot.transform);

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


        }

        public static void MakeStateGroup(GameObject go, Sector sector, QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
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
                if (Main.Debug) empty.AddComponent<BoxShapeVisualizer>();

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
            shuffle.Awake(); // this doesn't get called on its own for some reason. what? how?

            BoundsUtilities.AddBoundsVisibility(shuffleParent);
            shuffleParent.SetActive(true);
        }

        
        struct BoxShapeReciever
        {
            public MeshFilter f;
            public SkinnedMeshRenderer s;
            public GameObject g;
        }

        public static void AddBoundsVisibility(GameObject g)
        {
            var meshFilters = g.GetComponentsInChildren<MeshFilter>();
            var skinnedMeshRenderers = g.GetComponentsInChildren<SkinnedMeshRenderer>();
            
            var boxShapeRecievers = meshFilters
                .Select(f => new BoxShapeReciever() { f=f, g=f.gameObject })
                .Concat (
                    skinnedMeshRenderers.Select(s => new BoxShapeReciever() { s=s, g=s.gameObject })
                )
                .ToList();

            foreach(var boxshapeReciever in boxShapeRecievers)
            {
                var box = boxshapeReciever.g.AddComponent<BoxShape>();
                boxshapeReciever.g.AddComponent<ShapeVisibilityTracker>();
                if (Main.Debug) boxshapeReciever.g.AddComponent<BoxShapeVisualizer>();

                var fixer = boxshapeReciever.g.AddComponent<BoxShapeFixer>();
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

        void Update()
        {
            if (meshFilter == null && skinnedMeshRenderer == null) { NHLogger.LogVerbose("Useless BoxShapeFixer, destroying"); DestroyImmediate(this); }

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
