using OWML.Utils;
using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class DetectorBuilder
    {
        public static void Make(GameObject body, AstroObject primaryBody)
        {
            GameObject detectorGO = new GameObject("FieldDetector");
            detectorGO.SetActive(false);
            detectorGO.transform.parent = body.transform;
            detectorGO.transform.localPosition = Vector3.zero;
            detectorGO.layer = 20;

            ConstantForceDetector CFD = detectorGO.AddComponent<ConstantForceDetector>();

            GravityVolume parentGravityVolume = primaryBody.GetAttachedOWRigidbody().GetAttachedGravityVolume();

            CFD.SetValue("_detectableFields", new ForceVolume[] { parentGravityVolume });
            CFD.SetValue("_inheritElement0", true);

            detectorGO.SetActive(true);
        }
    }
}
