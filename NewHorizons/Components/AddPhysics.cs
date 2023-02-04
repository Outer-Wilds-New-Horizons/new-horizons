using System.Collections;
using UnityEngine;

namespace NewHorizons.Components;

/// <summary>
/// properly add physics to a detail
/// </summary>
[DisallowMultipleComponent]
public class AddPhysics : MonoBehaviour
{
    [Tooltip("The sector that the rigidbody will be simulated in, or none for it to always be on.")]
    public Sector Sector;
    [Tooltip("The mass of the physics object.\n" +
        "For reference, the player has a mass of 0.001 and the probe has a mass of 0.0001.")]
    public float Mass = 1f;
    [Tooltip("The radius that the added sphere collider will use for physics collision.\n" +
        "If there's already good colliders on the detail, you can make this 0.")]
    public float Radius = 1f;

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, Radius);
    }
}