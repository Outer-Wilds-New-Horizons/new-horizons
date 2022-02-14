using NewHorizons.External;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    static class RFVolumeBuilder
    {
        public static void Make(GameObject body, OWRigidbody rigidbody, float sphereOfInfluence)
        {
            GameObject rfGO = new GameObject("RFVolume");
            rfGO.transform.parent = body.transform;
            rfGO.transform.localPosition = Vector3.zero;
            rfGO.layer = 19;
            rfGO.SetActive(false);

            SphereCollider SC = rfGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            SC.radius = sphereOfInfluence * 2;

            ReferenceFrameVolume RFV = rfGO.AddComponent<ReferenceFrameVolume>();

            ReferenceFrame RV = new ReferenceFrame(rigidbody);
            RV._minSuitTargetDistance = sphereOfInfluence;
            RV._maxTargetDistance = 0;
            RV._autopilotArrivalDistance = 2.0f * sphereOfInfluence;
            RV._autoAlignmentDistance = sphereOfInfluence * 1.5f;
            
            RV._hideLandingModePrompt = false;
            RV._matchAngularVelocity = true;
            RV._minMatchAngularVelocityDistance = 70;
            RV._maxMatchAngularVelocityDistance = 400;
            RV._bracketsRadius = sphereOfInfluence;

            RFV._referenceFrame = RV;
            RFV._minColliderRadius = sphereOfInfluence;
            RFV._maxColliderRadius = sphereOfInfluence * 2f;
            RFV._isPrimaryVolume = true;
            RFV._isCloseRangeVolume = false;

            rfGO.SetActive(true);
        }
    }
}
