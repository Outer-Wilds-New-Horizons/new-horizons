using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using System.Collections;
using UnityEngine;

namespace NewHorizons.Components;

[DisallowMultipleComponent]
public class FixPhysics : MonoBehaviour
{
    private OWRigidbody _body;

    private void Awake()
    {
        _body = GetComponent<OWRigidbody>();
        _body._lastPosition = transform.position;
    }

    private void Start()
    {
        var parentBody = _body.GetOrigParentBody();
        if (parentBody == null) return;
        _body.SetVelocity(parentBody.GetPointVelocity(_body.GetWorldCenterOfMass()));
        _body.SetAngularVelocity(parentBody.GetAngularVelocity());
        if (_body._simulateInSector) _body.OnSectorOccupantsUpdated();
        var gravity = parentBody.GetComponentInChildren<GravityVolume>();
        if (gravity != null) gravity.GetComponent<OWTriggerVolume>().AddObjectToVolume(_body.GetComponentInChildren<ForceDetector>().gameObject);
    }
}
