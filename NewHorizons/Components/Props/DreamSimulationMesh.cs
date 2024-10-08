using NewHorizons.Utility.OuterWilds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Props
{
    public class DreamSimulationMesh : MonoBehaviour
    {
        Transform target;
        Material mat;
        MeshFilter sourceMeshFilter;
        MeshRenderer sourceMeshRenderer;
        MeshFilter targetMeshFilter;
        MeshRenderer targetMeshRenderer;
        StreamingRenderMeshHandle streamingHandle;

        bool initialized;

        public void Init(Transform target, Material mat)
        {
            this.target = target;
            this.mat = mat;

            gameObject.layer = Layer.DreamSimulation;
            
            sourceMeshFilter = target.GetComponent<MeshFilter>();
            sourceMeshRenderer = target.GetComponent<MeshRenderer>();
            targetMeshFilter = gameObject.AddComponent<MeshFilter>();
            targetMeshRenderer = gameObject.AddComponent<MeshRenderer>();

            transform.SetParent(target.transform, false);

            streamingHandle = target.GetComponent<StreamingRenderMeshHandle>();
            if (streamingHandle != null)
            {
                streamingHandle.OnMeshLoaded += OnMeshLoaded;
                streamingHandle.OnMeshUnloaded += OnMeshUnloaded;
            }

            initialized = true;
            Sync();
        }

        public void Sync()
        {
            if (!initialized) return;
            targetMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;
            targetMeshRenderer.sharedMaterials = sourceMeshRenderer.sharedMaterials.Select(_ => mat).ToArray();
            targetMeshRenderer.enabled = sourceMeshRenderer.enabled;
        }

        void OnDestroy()
        {
            if (streamingHandle != null)
            {
                streamingHandle.OnMeshLoaded -= OnMeshLoaded;
                streamingHandle.OnMeshUnloaded -= OnMeshUnloaded;
            }
        }

        void OnMeshLoaded()
        {
            Sync();
        }

        void OnMeshUnloaded()
        {
            Sync();
        }
    }
}
