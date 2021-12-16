using NewHorizons.External;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class AmbientLightBuilder
    {
        public static void Make(GameObject body, Sector sector, MColor32 lightTint, float scale)
        {
            GameObject lightGO = new GameObject("Lights");
            lightGO.SetActive(false);
            lightGO.transform.parent = body.transform;
            
            Light L = lightGO.AddComponent<Light>();
            L.type = LightType.Point;
            L.range = scale + 10;
            L.color = (lightTint != null) ? lightTint.ToColor32() : (Color32)Color.black;
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
