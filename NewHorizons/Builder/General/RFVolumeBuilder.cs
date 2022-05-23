using NewHorizons.External.Configs;
using UnityEngine;
namespace NewHorizons.Builder.General
{
    public static class RFVolumeBuilder
    {
        public static void Make(GameObject planetGO, OWRigidbody owRigidBody, float sphereOfInfluence)
        {
            var referenceFrameGO = new GameObject("RFVolume");
            referenceFrameGO.transform.parent = planetGO.transform;
            referenceFrameGO.transform.localPosition = Vector3.zero;
            referenceFrameGO.layer = 19;
            referenceFrameGO.SetActive(false);

            var sphereCollider = referenceFrameGO.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = sphereOfInfluence * 2;

            var referenceFrameVolume = referenceFrameGO.AddComponent<ReferenceFrameVolume>();

            var referenceFrame = new ReferenceFrame(owRigidBody);
            referenceFrame._minSuitTargetDistance = sphereOfInfluence;
            referenceFrame._maxTargetDistance = 0;
            referenceFrame._autopilotArrivalDistance = 2.0f * sphereOfInfluence;
            referenceFrame._autoAlignmentDistance = sphereOfInfluence * 1.5f;

            referenceFrame._hideLandingModePrompt = false;
            referenceFrame._matchAngularVelocity = true;
            referenceFrame._minMatchAngularVelocityDistance = 70;
            referenceFrame._maxMatchAngularVelocityDistance = 400;
            referenceFrame._bracketsRadius = sphereOfInfluence;

            referenceFrameVolume._referenceFrame = referenceFrame;
            referenceFrameVolume._minColliderRadius = sphereOfInfluence;
            referenceFrameVolume._maxColliderRadius = sphereOfInfluence * 2f;
            referenceFrameVolume._isPrimaryVolume = true;
            referenceFrameVolume._isCloseRangeVolume = false;

            referenceFrameGO.SetActive(true);
        }
    }
}
