using NewHorizons.Builder.General;
using NewHorizons.External.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
