using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using UnityEngine;
namespace NewHorizons.Builder.General
{
    public static class RFVolumeBuilder
    {
        public static void Make(GameObject planetGO, OWRigidbody owrb, float sphereOfInfluence, ReferenceFrameModule module)
        {
            var rfGO = new GameObject("RFVolume");
            rfGO.transform.parent = planetGO.transform;
            rfGO.transform.localPosition = Vector3.zero;
            rfGO.layer = 19;
            rfGO.SetActive(false);

            var SC = rfGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            SC.radius = sphereOfInfluence * 2;

            var RFV = rfGO.AddComponent<ReferenceFrameVolume>();

            var RV = new ReferenceFrame(owrb);
            RV._minSuitTargetDistance = module.targetWhenClose ? 0 : sphereOfInfluence;
            RV._maxTargetDistance = 0;
            RV._autopilotArrivalDistance = 2.0f * sphereOfInfluence;
            RV._autoAlignmentDistance = sphereOfInfluence * 1.5f;

            RV._hideLandingModePrompt = false;
            RV._matchAngularVelocity = true;
            RV._minMatchAngularVelocityDistance = 70;
            RV._maxMatchAngularVelocityDistance = 400;
            RV._bracketsRadius = module.bracketRadius > -1 ? module.bracketRadius : sphereOfInfluence;

            RFV._referenceFrame = RV;
            RFV._minColliderRadius = sphereOfInfluence;
            RFV._maxColliderRadius = sphereOfInfluence * 2f;
            RFV._isPrimaryVolume = true;
            RFV._isCloseRangeVolume = false;

            owrb.SetAttachedReferenceFrameVolume(RFV);

            rfGO.SetActive(!module.hideInMap);
        }
    }
}
