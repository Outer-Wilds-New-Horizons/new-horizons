using NewHorizons.External;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class RFVolumeBuilder
    {
        public static void Make(GameObject body, OWRigidbody rigidbody, float atmoEndSize)
        {
            GameObject rfGO = new GameObject("RFVolume");
            rfGO.transform.parent = body.transform;
            rfGO.layer = 19;
            rfGO.SetActive(false);

            SphereCollider SC = rfGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            SC.radius = atmoEndSize * 2;

            ReferenceFrameVolume RFV = rfGO.AddComponent<ReferenceFrameVolume>();

            ReferenceFrame RV = new ReferenceFrame(rigidbody);
            RV.SetValue("_minSuitTargetDistance", 300);
            RV.SetValue("_maxTargetDistance", 0);
            RV.SetValue("_autopilotArrivalDistance", atmoEndSize * 2f);
            RV.SetValue("_autoAlignmentDistance", atmoEndSize * 1.5f);
            //Utility.AddDebugShape.AddSphere(rfGO, 1000, new Color32(0, 255, 0, 128));
            RV.SetValue("_hideLandingModePrompt", false);
            RV.SetValue("_matchAngularVelocity", true);
            RV.SetValue("_minMatchAngularVelocityDistance", 70);
            RV.SetValue("_maxMatchAngularVelocityDistance", 400);
            RV.SetValue("_bracketsRadius", 300);

            RFV.SetValue("_referenceFrame", RV);
            RFV.SetValue("_minColliderRadius", 300);
            RFV.SetValue("_maxColliderRadius", atmoEndSize * 2f);
            RFV.SetValue("_isPrimaryVolume", true);
            RFV.SetValue("_isCloseRangeVolume", false);

            rfGO.SetActive(true);
            Logger.Log("Finished building rfvolume", Logger.LogType.Log);
        }
    }
}
