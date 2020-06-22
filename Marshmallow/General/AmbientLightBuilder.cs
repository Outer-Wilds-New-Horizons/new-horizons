using Marshmallow.External;
using UnityEngine;
using Logger = Marshmallow.Utility.Logger;

namespace Marshmallow.General
{
    static class AmbientLightBuilder
    {
        public static void Make(GameObject body, Sector sector, IPlanetConfig config)
        {
            GameObject lightGO = new GameObject();
            lightGO.SetActive(false);
            lightGO.transform.parent = body.transform;

            Light L = lightGO.AddComponent<Light>();
            L.type = LightType.Point;
            L.range = config.GroundSize * 2;
            L.color = config.LightTint.ToColor32();
            L.intensity = 0.8f;
            L.shadows = LightShadows.None;
            L.cookie = GameObject.Find("AmbientLight_GD").GetComponent<Light>().cookie;

            SectorLightsCullGroup SLCG = lightGO.AddComponent<SectorLightsCullGroup>();
            SLCG.SetSector(sector);

            lightGO.SetActive(true);

            Logger.Log("Finished building ambient light", Logger.LogType.Log);
        }
    }
}
