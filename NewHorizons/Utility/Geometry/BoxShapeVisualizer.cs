using UnityEngine;

namespace NewHorizons.Utility.Geometry
{
    public class BoxShapeVisualizer : MonoBehaviour
    {
        BoxShape box;

        void Awake()
        {
            box = GetComponent<BoxShape>();
        }

        void OnRenderObject()
        {
            Popcron.Gizmos.Cube(transform.TransformPoint(box.center), transform.rotation, Vector3.Scale(box.size, transform.lossyScale));
        }
    }
}
