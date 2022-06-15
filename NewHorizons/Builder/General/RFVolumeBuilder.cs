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
            // This radius ends up being set by min and max collider radius on the RFV but we set it anyway because why fix what aint broke
            SC.radius = sphereOfInfluence * 2;

            var RFV = rfGO.AddComponent<ReferenceFrameVolume>();

            var minTargetDistance = module.targetWhenClose ? 0 : sphereOfInfluence;

            var RV = new ReferenceFrame(owrb);
            RV._minSuitTargetDistance = minTargetDistance;
            // The game raycasts to 100km, but if the target is farther than this max distance it ignores it
            RV._maxTargetDistance = module.maxTargetDistance;
            RV._autopilotArrivalDistance = 2.0f * sphereOfInfluence;
            RV._autoAlignmentDistance = sphereOfInfluence * 1.5f;

            RV._hideLandingModePrompt = false;
            RV._matchAngularVelocity = true;
            RV._minMatchAngularVelocityDistance = 70;
            RV._maxMatchAngularVelocityDistance = 400;
            RV._bracketsRadius = module.bracketRadius > -1 ? module.bracketRadius : sphereOfInfluence;

            RFV._referenceFrame = RV;
            RFV._minColliderRadius = minTargetDistance;
            RFV._maxColliderRadius = module.targetColliderRadius > 0 ? module.targetColliderRadius : sphereOfInfluence * 2f;
            RFV._isPrimaryVolume = true;
            RFV._isCloseRangeVolume = false;

            owrb.SetAttachedReferenceFrameVolume(RFV);

            rfGO.SetActive(!module.hideInMap);
        }
    }
}
