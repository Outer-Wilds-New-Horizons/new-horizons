using System.Collections;
using UnityEngine;

namespace NewHorizons.Components
{
    /// <summary>
    /// properly add physics to a detail
    /// </summary>
    public class AddPhysics : MonoBehaviour
    {
        public Sector Sector;
        public float Mass;
        public float Radius;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(.1f);

            var parentBody = GetComponentInParent<OWRigidbody>();

            // hack: make all mesh colliders convex
            // triggers are already convex
            // prints errors for non readable meshes but whatever
            foreach (var meshCollider in GetComponentsInChildren<MeshCollider>(true))
                meshCollider.convex = true;

            var bodyGo = new GameObject($"{name}_Body");
            bodyGo.SetActive(false);
            bodyGo.transform.position = transform.position;
            bodyGo.transform.rotation = transform.rotation;

            var owRigidbody = bodyGo.AddComponent<OWRigidbody>();
            owRigidbody._simulateInSector = Sector;

            bodyGo.layer = LayerMask.NameToLayer("PhysicalDetector");
            bodyGo.tag = "DynamicPropDetector";
            // this collider is not included in groups. oh well
            bodyGo.AddComponent<SphereCollider>().radius = Radius;
            bodyGo.AddComponent<DynamicForceDetector>();
            bodyGo.AddComponent<DynamicFluidDetector>();

            bodyGo.SetActive(true);

            transform.parent = bodyGo.transform;
            owRigidbody.SetMass(Mass);
            owRigidbody.SetVelocity(parentBody.GetPointVelocity(transform.position));

            Destroy(this);
        }
    }
}