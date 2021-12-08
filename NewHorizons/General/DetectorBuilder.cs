using OWML.Utils;
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
            detectorGO.layer = 20;

            ConstantForceDetector CFD = detectorGO.AddComponent<ConstantForceDetector>();
            ForceVolume[] temp = new ForceVolume[1];
            temp[0] = primaryBody.GetAttachedOWRigidbody().GetAttachedGravityVolume();
            CFD.SetValue("_detectableFields", temp);
            CFD.SetValue("_inheritElement0", true);

            detectorGO.SetActive(true);
            Logger.Log("Finished building detector", Logger.LogType.Log);
        }
    }
}
