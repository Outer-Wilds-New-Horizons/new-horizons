using UnityEngine;

namespace NewHorizons.Utility.Geometry
{
    public class BoxShapeVisualizer : MonoBehaviour
    {
        private BoxShape _box;

        public void Awake()
        {
            _box = GetComponent<BoxShape>();
        }

        public void OnRenderObject()
        {
            if (Main.Debug && Main.VisualizeQuantumObjects)
            {
                Popcron.Gizmos.Cube(transform.TransformPoint(_box.center), transform.rotation, Vector3.Scale(_box.size, transform.lossyScale));
            }
        }
    }
}
