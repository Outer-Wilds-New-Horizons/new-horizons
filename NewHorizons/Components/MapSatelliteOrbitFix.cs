using NewHorizons.Builder.General;
using NewHorizons.External.Configs;
using UnityEngine;
namespace NewHorizons.Components
{
    public class MapSatelliteOrbitFix : MonoBehaviour
    {
        public void Awake()
        {
            // TODO: eventually all bodies should have configs
            var config = new PlanetConfig(null);
            config.Base.SurfaceSize = 10f;

            var detector = base.transform.GetComponentInChildren<DynamicForceDetector>();
            var ao = base.GetComponent<AstroObject>();
            var newDetector = DetectorBuilder.Make(base.gameObject, ao.GetAttachedOWRigidbody(), ao.GetPrimaryBody(), ao, config);
            newDetector.transform.parent = detector.transform.parent;
            GameObject.Destroy(detector);
        }
    }
}
