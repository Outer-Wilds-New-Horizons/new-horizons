using NewHorizons.Builder.General;
using NewHorizons.External.Configs;
using UnityEngine;
namespace NewHorizons.Components.Fixers
{
    public class MapSatelliteOrbitFix : MonoBehaviour
    {
        public void Awake()
        {
            var config = new PlanetConfig
            {
                Base =
                {
                    surfaceSize = 10f
                }
            };

            var detector = transform.GetComponentInChildren<DynamicForceDetector>();
            var ao = GetComponent<AstroObject>();
            var newDetector = DetectorBuilder.Make(gameObject, ao.GetAttachedOWRigidbody(), ao.GetPrimaryBody(), ao, config);
            newDetector.transform.parent = detector.transform.parent;
            Destroy(detector);
        }
    }
}
