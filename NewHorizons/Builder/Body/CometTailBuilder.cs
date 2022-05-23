using NewHorizons.External.Configs;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public static class CometTailBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, PlanetConfig config)
        {
            var cometTail = Object.Instantiate(GameObject.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes"),
                sector?.transform ?? planetGO.transform);
            cometTail.transform.position = planetGO.transform.position;
            cometTail.name = "CometTail";
            cometTail.transform.localScale = Vector3.one * config.Base.surfaceSize / 110;

            var alignment = new Vector3(0, 270, 90);
            if (config.Base.cometTailRotation != null) alignment = config.Base.cometTailRotation;

            cometTail.transform.rotation = Quaternion.Euler(alignment);
        }
    }
}