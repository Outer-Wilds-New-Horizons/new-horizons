using NewHorizons.External.Modules;
using NewHorizons.Utility.OuterWilds;
using UnityEngine;

namespace NewHorizons.Builder.General
{
    public static class RFVolumeBuilder
    {
        public static GameObject Make(GameObject planetGO, OWRigidbody owrb, float sphereOfInfluence, ReferenceFrameModule module)
        {
            if (!module.enabled)
            {
                // We can't not build a reference frame volume, Cloak requires one to be there
                module.maxTargetDistance = 0f;
                module.targetWhenClose = true;
                module.targetColliderRadius = 0.001f;
                module.hideInMap = true;
                owrb.SetIsTargetable(false);
            }
            
            var rfGO = new GameObject("RFVolume");
            rfGO.transform.parent = planetGO.transform;
            rfGO.transform.localPosition = Vector3.zero;
            rfGO.layer = Layer.ReferenceFrameVolume;
            rfGO.SetActive(false);

            var SC = rfGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            // This radius ends up being set by min and max collider radius on the RFV but we set it anyway because why fix what aint broke
            SC.radius = sphereOfInfluence * 2;

            float targetDistance = module.maxTargetDistance;

            ReferenceFrameVolume RFV;
            if (module.hideInMap)
            {
                var mapRFV = rfGO.AddComponent<MapReferenceFrameVolume>();
                mapRFV._defaultMaxTargetDistance = targetDistance;
                mapRFV._mapMaxTargetDistance = 0.001f; // Setting to 0 makes it targetable at any distance, so lets make this as small as possible.
                RFV = mapRFV;
            }
            else
                RFV = rfGO.AddComponent<ReferenceFrameVolume>();

            var minTargetDistance = module.targetWhenClose ? 0 : sphereOfInfluence;

            var RV = new ReferenceFrame(owrb);
            RV._minSuitTargetDistance = minTargetDistance;
            // The game raycasts to 100km, but if the target is farther than this max distance it ignores it
            RV._maxTargetDistance = targetDistance;
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

            if (module.localPosition != null)
            {
                rfGO.transform.localPosition = module.localPosition;
                RV._localPosition = module.localPosition;
                RV._useCenterOfMass = false;
            }

            owrb.SetAttachedReferenceFrameVolume(RFV);

            rfGO.SetActive(true);
            return rfGO;
        }
    }
}
