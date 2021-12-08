using NewHorizons.External;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class AmbientLightBuilder
    {
        public static void Make(GameObject body, Sector sector, IPlanetConfig config)
        {
            GameObject lightGO = new GameObject("Lights");
            lightGO.SetActive(false);
            lightGO.transform.parent = body.transform;
            
            Light L = lightGO.AddComponent<Light>();
            L.type = LightType.Point;
            L.range = config.AtmoEndSize + 10;
            L.color = (config.LightTint != null) ? config.LightTint.ToColor32() : (Color32)Color.black;
            L.intensity = 0.8f;
            L.shadows = LightShadows.None;

            L.cookie = GameObject.Find("/GiantsDeep_Body/AmbientLight_GD").GetComponent<Light>().cookie;

            SectorLightsCullGroup SLCG = lightGO.AddComponent<SectorLightsCullGroup>();
            SLCG.SetSector(sector);

            lightGO.SetActive(true);

            Logger.Log("Finished building ambient light", Logger.LogType.Log);
        }
    }
}
