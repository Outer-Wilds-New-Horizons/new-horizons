#region

using UnityEngine;

#endregion

namespace NewHorizons.Components
{
    public class NHFluidVolume : RadialFluidVolume
    {
        public override float GetDepthAtPosition(Vector3 worldPosition)
        {
            var vector = transform.InverseTransformPoint(worldPosition);
            var dist = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z + vector.y * vector.y);
            return dist - _radius;
        }
    }
}