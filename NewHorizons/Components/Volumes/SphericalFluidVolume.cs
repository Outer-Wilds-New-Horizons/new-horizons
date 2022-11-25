using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    public class SphericalFluidVolume : FluidVolume
    {
        public float radius;

        public virtual void OnValidate()
        {
            SphereCollider collider = GetComponent<SphereCollider>();
            if (collider != null) collider.radius = radius;

            SphereShape shape = GetComponent<SphereShape>();
            if (shape != null) shape.radius = radius;
        }

        public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector) => _attachedBody.GetPointVelocity(worldPosition);

        public override bool IsSpherical() => true;

        public override float GetFractionSubmerged(FluidDetector detector) => detector.GetBuoyancyData().CalculateSubmergedFraction((detector.transform.position - transform.position).magnitude, radius);

        public virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
