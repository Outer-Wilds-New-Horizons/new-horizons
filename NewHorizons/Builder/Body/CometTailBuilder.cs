using NewHorizons.External;
using NewHorizons.External.Configs;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public static class CometTailBuilder
    {
        public static void Make(GameObject go, IPlanetConfig config, AstroObject primary)
        {
            var cometTail = GameObject.Instantiate(GameObject.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes"), go.transform);
            cometTail.transform.localPosition = Vector3.zero;
            cometTail.name = "CometTail";
            cometTail.transform.localScale = Vector3.one * config.Base.SurfaceSize / 110;

            Vector3 alignmentAxis = new Vector3(0, -1, 0);
            if (config.Orbit.AlignmentAxis != null) alignmentAxis = config.Orbit.AlignmentAxis;

            cometTail.transform.localRotation = Quaternion.Euler(0, 270, 90) * Quaternion.FromToRotation(new Vector3(0, -1, 0), alignmentAxis);
        }
    }
}
