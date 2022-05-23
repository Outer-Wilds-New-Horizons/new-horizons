using UnityEngine;
namespace NewHorizons.Components
{
    public class NHFluidVolume : RadialFluidVolume
    {
        public override float GetDepthAtPosition(Vector3 worldPosition)
        {
            Vector3 vector = transform.InverseTransformPoint(worldPosition);
            float dist = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z + vector.y * vector.y);
            return dist - _radius;
        }
    }
}
