using NewHorizons.External;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
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
            RV.SetValue("_minSuitTargetDistance", sphereOfInfluence);
            RV.SetValue("_maxTargetDistance", 0);
            RV.SetValue("_autopilotArrivalDistance", sphereOfInfluence * 2f);
            RV.SetValue("_autoAlignmentDistance", sphereOfInfluence * 1.5f);

            RV.SetValue("_hideLandingModePrompt", false);
            RV.SetValue("_matchAngularVelocity", true);
            RV.SetValue("_minMatchAngularVelocityDistance", 70);
            RV.SetValue("_maxMatchAngularVelocityDistance", 400);
            RV.SetValue("_bracketsRadius", sphereOfInfluence);

            RFV.SetValue("_referenceFrame", RV);
            RFV.SetValue("_minColliderRadius", sphereOfInfluence);
            RFV.SetValue("_maxColliderRadius", sphereOfInfluence * 2f);
            RFV.SetValue("_isPrimaryVolume", true);
            RFV.SetValue("_isCloseRangeVolume", false);

            rfGO.SetActive(true);
        }
    }
}
