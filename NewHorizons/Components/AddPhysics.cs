using NewHorizons.Utility.OuterWilds;
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
        "Most pushable props use the default value, which matches the player mass.")]
    public float Mass = 0.001f;
    [Tooltip("The radius that the added sphere collider will use for physics collision.\n" +
        "If there's already good colliders on the detail, you can make this 0.")]
    public float Radius = 1f;
    [Tooltip("If true, this detail will stay still until it touches something.\n" +
        "Good for zero-g props.")]
    public bool SuspendUntilImpact;

    private OWRigidbody _body;
    private ImpactSensor _impactSensor;

    private IEnumerator Start()
    {
        // detectors dont detect unless we wait for some reason
        yield return new WaitForSeconds(.1f);

        var parentBody = GetComponentInParent<OWRigidbody>();

        // hack: make all mesh colliders convex
        // triggers are already convex
        // doesnt work for some non readable meshes but whatever
        foreach (var meshCollider in GetComponentsInChildren<MeshCollider>(true))
            meshCollider.convex = true;

        var bodyGo = new GameObject($"{name}_Body");
        bodyGo.SetActive(false);
        bodyGo.transform.parent = transform.parent;
        bodyGo.transform.position = transform.position;
        bodyGo.transform.rotation = transform.rotation;

        _body = bodyGo.AddComponent<OWRigidbody>();
        _body._simulateInSector = Sector;

        bodyGo.layer = Layer.PhysicalDetector;
        bodyGo.tag = "DynamicPropDetector";
        // this collider is not included in groups. oh well
        bodyGo.AddComponent<SphereCollider>().radius = Radius;
        var shape = bodyGo.AddComponent<SphereShape>();
        shape._collisionMode = Shape.CollisionMode.Detector;
        shape._layerMask = (int)(Shape.Layer.Default | Shape.Layer.Gravity);
        shape._radius = Radius;
        bodyGo.AddComponent<DynamicForceDetector>();
        var fluidDetector = bodyGo.AddComponent<DynamicFluidDetector>();
        fluidDetector._buoyancy = Locator.GetProbe().GetOWRigidbody()._attachedFluidDetector._buoyancy;
        fluidDetector._splashEffects = Locator.GetProbe().GetOWRigidbody()._attachedFluidDetector._splashEffects;

        _impactSensor = bodyGo.AddComponent<ImpactSensor>();
        var audioSource = bodyGo.AddComponent<AudioSource>();
        audioSource.maxDistance = 30;
        audioSource.dopplerLevel = 0;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1;
        var owAudioSource = bodyGo.AddComponent<OWAudioSource>();
        owAudioSource._audioSource = audioSource;
        owAudioSource._track = OWAudioMixer.TrackName.Environment;
        var objectImpactAudio = bodyGo.AddComponent<ObjectImpactAudio>();
        objectImpactAudio._minPitch = 0.4f;
        objectImpactAudio._maxPitch = 0.6f;
        objectImpactAudio._impactSensor = _impactSensor;

        bodyGo.SetActive(true);

        transform.parent = bodyGo.transform;
        _body.SetMass(Mass);
        _body.SetVelocity(parentBody.GetPointVelocity(_body.GetWorldCenterOfMass()));
        _body.SetAngularVelocity(parentBody.GetAngularVelocity());

        // #536 - Physics objects in bramble dimensions not disabled on load
        // sectors wait 3 frames and then call OnSectorOccupantsUpdated
        // however we wait .1 real seconds which is longer
        // so we have to manually call this
        if (_body._simulateInSector) _body.OnSectorOccupantsUpdated();

        if (SuspendUntilImpact)
        {
            // copied from OWRigidbody.Suspend
            _body._suspensionBody = parentBody;
            _body.MakeKinematic();
            _body._transform.parent = parentBody.transform;
            _body._suspended = true;
            _body._unsuspendNextUpdate = false;

            // match velocity doesnt work so just make it not targetable
            _body.SetIsTargetable(false);

            _impactSensor.OnImpact += OnImpact;
            Locator.GetProbe().OnAnchorProbe += OnAnchorProbe;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_impactSensor) _impactSensor.OnImpact -= OnImpact;
        Locator.GetProbe().OnAnchorProbe -= OnAnchorProbe;
    }

    private void OnImpact(ImpactData impact)
    {
        _body.UnsuspendImmediate(false);
        _body.SetIsTargetable(true);
        Destroy(this);
    }

    private void OnAnchorProbe()
    {
        var attachedOWRigidbody = Locator.GetProbe().GetAttachedOWRigidbody(true);
        if (attachedOWRigidbody == _body)
        {
            OnImpact(null);

            // copied from ProbeAnchor.AnchorToObject
            var _probeBody = Locator.GetProbe().GetOWRigidbody();
            var hitPoint = _probeBody.GetWorldCenterOfMass();
            if (attachedOWRigidbody.GetMass() < 100f)
            {
                Vector3 vector = _probeBody.GetVelocity() - attachedOWRigidbody.GetPointVelocity(_probeBody.GetPosition());
                attachedOWRigidbody.GetRigidbody().AddForceAtPosition(vector.normalized * 0.005f, hitPoint, ForceMode.Impulse);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
