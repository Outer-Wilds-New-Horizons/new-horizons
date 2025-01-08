using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Utility.Geometry;

internal class BoxShapeFixer : MonoBehaviour
{
    public BoxShape shape;
    public MeshFilter meshFilter;
    public SkinnedMeshRenderer skinnedMeshRenderer;

    public void Update()
    {
        if (meshFilter == null && skinnedMeshRenderer == null)
        {
            NHLogger.LogVerbose("Useless BoxShapeFixer, destroying"); DestroyImmediate(this);
        }

        Mesh sharedMesh = null;
        if (meshFilter != null)
        {
            sharedMesh = meshFilter.sharedMesh;
        }
        if (skinnedMeshRenderer != null)
        {
            sharedMesh = skinnedMeshRenderer.sharedMesh;
        }

        if (sharedMesh == null)
        {
            return;
        }
        if (sharedMesh.bounds.size == Vector3.zero)
        {
            return;
        }

        shape.size = sharedMesh.bounds.size;
        shape.center = sharedMesh.bounds.center;

        DestroyImmediate(this);
    }
}
