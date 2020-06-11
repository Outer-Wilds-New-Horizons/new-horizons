using UnityEngine;

namespace Marshmallow.General
{
    static class MakeAmbientLight
    {
        public static void Make(GameObject body, Sector sector)
        {
            GameObject lightGO = new GameObject();
            lightGO.SetActive(false);
            lightGO.transform.parent = body.transform;

            Light L = lightGO.AddComponent<Light>();
            L.type = LightType.Point;
            L.range = 700f;
            L.color = Color.red;
            L.intensity = 0.8f;
            L.shadows = LightShadows.None;
            L.cookie = GameObject.Find("AmbientLight_GD").GetComponent<Light>().cookie;

            SectorLightsCullGroup SLCG = lightGO.AddComponent<SectorLightsCullGroup>();
            SLCG.SetSector(sector);

            lightGO.SetActive(true);
        }
    }
}
