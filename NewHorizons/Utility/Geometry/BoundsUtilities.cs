using System.Linq;
using UnityEngine;

namespace NewHorizons.Utility.Geometry;

public static class BoundsUtilities
{
    private struct BoxShapeReciever
    {
        public MeshFilter meshFilter;
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public GameObject gameObject;
    }

    public static void AddBoundsVisibility(GameObject gameObject)
    {
        var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        var skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

        var boxShapeRecievers = meshFilters
            .Select(meshFilter => new BoxShapeReciever() { meshFilter = meshFilter, gameObject = meshFilter.gameObject })
            .Concat(
                skinnedMeshRenderers.Select(skinnedMeshRenderer => new BoxShapeReciever() { skinnedMeshRenderer = skinnedMeshRenderer, gameObject = skinnedMeshRenderer.gameObject })
            )
            .ToList();

        foreach (var boxshapeReciever in boxShapeRecievers)
        {
            var box = boxshapeReciever.gameObject.AddComponent<BoxShape>();
            boxshapeReciever.gameObject.AddComponent<ShapeVisibilityTracker>();
            if (Main.Debug)
            {
                boxshapeReciever.gameObject.AddComponent<BoxShapeVisualizer>();
            }

            var fixer = boxshapeReciever.gameObject.AddComponent<BoxShapeFixer>();
            fixer.shape = box;
            fixer.meshFilter = boxshapeReciever.meshFilter;
            fixer.skinnedMeshRenderer = boxshapeReciever.skinnedMeshRenderer;
        }
    }

    public static Bounds GetBoundsOfSelfAndChildMeshes(GameObject gameObject)
    {
        var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        var corners = meshFilters.SelectMany(meshFilter => GetMeshCorners(meshFilter, gameObject)).ToList();

        var bounds = new Bounds(corners[0], Vector3.zero);
        corners.ForEach(bounds.Encapsulate);

        return bounds;
    }

    public static Vector3[] GetMeshCorners(MeshFilter meshFilter, GameObject relativeTo = null)
    {
        var bounds = meshFilter.mesh.bounds;

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

        var globalCorners = localCorners.Select(meshFilter.transform.TransformPoint).ToArray();

        if (relativeTo == null) return globalCorners;

        return globalCorners.Select(relativeTo.transform.InverseTransformPoint).ToArray();
    }
}
