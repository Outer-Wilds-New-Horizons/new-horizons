using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Utility.Geometry;

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
