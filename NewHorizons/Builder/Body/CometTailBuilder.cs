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
        public static void Make(GameObject planetGO, Sector sector, IPlanetConfig config, AstroObject primary)
        {
            var cometTail = GameObject.Instantiate(GameObject.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes"), sector?.transform ?? planetGO.transform);
            cometTail.transform.position = planetGO.transform.position;
            cometTail.name = "CometTail";
            cometTail.transform.localScale = Vector3.one * config.Base.SurfaceSize / 110;

            Vector3 alignment = new Vector3(0, 270, 90);
            if (config.Base.CometTailRotation != null) alignment = config.Base.CometTailRotation;

            cometTail.transform.rotation = Quaternion.Euler(alignment);
        }
    }
}
