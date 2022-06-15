using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class QuantumBuilder
    {
        
        public static void Make(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod, PropModule.QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            // if a prop doesn't have a visibiilty volume, create a box volume using the prop's mesh bounds (if there are multiple mesh filters, use the min/max bounds accross all meshes)
            switch(quantumGroup.type)
            {
                case PropModule.QuantumGroupType.Sockets: MakeSocketGroup (go, sector, config, mod, quantumGroup, propsInGroup); return;
                case PropModule.QuantumGroupType.States:  MakeStateGroup  (go, sector, config, mod, quantumGroup, propsInGroup); return;
                case PropModule.QuantumGroupType.Shuffle: MakeShuffleGroup(go, sector, config, mod, quantumGroup, propsInGroup); return;
                // TODO: for quantum socket group allow specifying an _emptySocketObject
            }
        }
        
        public static void MakeSocketGroup(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod, PropModule.QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            // note: for the visibility boxes on quantum sockets, if there's only one prop that's part of this group, clone its visibility volume
            // otherwise, create a box according to the max and min dimensions across all props in this group (ie the box should be able to fit inside of it the visibility volume on any prop in this group)
            
            var groupRoot = new GameObject("Quantum Sockets - " + quantumGroup.id);
            groupRoot.transform.parent = sector.transform;
            groupRoot.transform.localPosition = Vector3.zero;
            
            var sockets = new List<QuantumSocket>(quantumGroup.sockets.Length);
            for (int i = 0; i < quantumGroup.sockets.Length; i++)
            {
                var socketInfo = quantumGroup.sockets[i];

                var socket = new GameObject("Socket " + i);
                socket.transform.parent = groupRoot.transform;
                socket.transform.localPosition = socketInfo.position;
                socket.transform.localEulerAngles = socketInfo.rotation;

                sockets[i] = socket.AddComponent<QuantumSocket>();
            }

            foreach(var prop in propsInGroup)
            {
                var quantumObject = prop.AddComponent<SocketedQuantumObject>();
                quantumObject._socketRoot = groupRoot;
                quantumObject._socketList = sockets;
                
                if (quantumObject.gameObject.GetComponentInChildren<VisibilityTracker>() != null) continue;

                var boxBounds = GetBoundsOfSelfAndChildMeshes(prop);
                var boxShape = prop.AddComponent<BoxShape>();
                boxShape.center = boxBounds.center;
                boxShape.extents = boxBounds.size;
                
                prop.AddComponent<VisibilityTracker>();
            }
        }

        public static void MakeStateGroup(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod, PropModule.QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            
        }

        public static void MakeShuffleGroup(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod, PropModule.QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            // if the type is shuffle, create a new game object that has a location average of all props' locations, and put a visibility volume on it that all props' visibility volumes fit inside of
            // then, make all props children of it
            
        }


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

            return globalCorners.Select(globalCorner => relativeTo.transform.TransformPoint(globalCorner)).ToArray();
        }
    }
}
