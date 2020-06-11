using Marshmallow.External;
using OWML.ModHelper.Events;
using UnityEngine;

namespace Marshmallow.General
{
    static class MakeFieldDetector
    {
        public static void Make(GameObject body, AstroObject primaryBody, IPlanetConfig config)
        {
            GameObject detectorGO = new GameObject();
            detectorGO.SetActive(false);
            detectorGO.name = "FieldDetector";
            detectorGO.transform.parent = body.transform;
            detectorGO.layer = 20;

            ConstantForceDetector CFD = detectorGO.AddComponent<ConstantForceDetector>();
            ForceVolume[] temp = new ForceVolume[1];
            temp[0] = primaryBody.GetAttachedOWRigidbody().GetAttachedGravityVolume();
            CFD.SetValue("_detectableFields", temp);
            CFD.SetValue("_inheritElement0", config.IsMoon);

            detectorGO.SetActive(true);
        }
    }
}
