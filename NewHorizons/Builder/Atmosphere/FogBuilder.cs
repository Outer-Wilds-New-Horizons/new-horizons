#region

using NewHorizons.External.Modules;
using UnityEngine;

#endregion

namespace NewHorizons.Builder.Atmosphere
{
    public static class FogBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmo)
        {
            var fogGO = new GameObject("FogSphere");
            fogGO.SetActive(false);
            fogGO.transform.parent = sector?.transform ?? planetGO.transform;
            fogGO.transform.localScale = Vector3.one;

            // Going to copy from dark bramble
            var dbFog = GameObject.Find("DarkBramble_Body/Atmosphere_DB/FogLOD");
            var dbPlanetaryFogController = GameObject.Find("DarkBramble_Body/Atmosphere_DB/FogSphere_DB")
                .GetComponent<PlanetaryFogController>();
            var brambleLODFog = GameObject.Find("DarkBramble_Body/Sector_DB/Proxy_DB/LOD_DB_VolumeticFog");

            var MF = fogGO.AddComponent<MeshFilter>();
            MF.mesh = dbFog.GetComponent<MeshFilter>().mesh;

            var MR = fogGO.AddComponent<MeshRenderer>();
            MR.materials = dbFog.GetComponent<MeshRenderer>().materials;
            MR.allowOcclusionWhenDynamic = true;

            var PFC = fogGO.AddComponent<PlanetaryFogController>();
            PFC.fogLookupTexture = dbPlanetaryFogController.fogLookupTexture;
            PFC.fogRadius = atmo.fogSize;
            PFC.fogDensity = atmo.fogDensity;
            PFC.fogExponent = 1f;
            PFC.fogColorRampTexture = dbPlanetaryFogController.fogColorRampTexture;
            PFC.fogColorRampIntensity = 1f;
            PFC.fogTint = atmo.fogTint;

            var lodFogGO = new GameObject("LODFogSphere");
            lodFogGO.SetActive(false);
            lodFogGO.transform.parent = fogGO.transform;
            lodFogGO.transform.localScale = Vector3.one * atmo.size / 320f;

            var lodMF = lodFogGO.AddComponent<MeshFilter>();
            lodMF.mesh = brambleLODFog.GetComponent<MeshFilter>().mesh;

            var lodMR = lodFogGO.AddComponent<MeshRenderer>();
            lodMR.material = new Material(brambleLODFog.GetComponent<MeshRenderer>().material);
            lodMR.material.color = atmo.fogTint;
            lodMR.material.renderQueue = 1000;

            /*
            SectorProxy lodFogSectorProxy = lodFogGO.AddComponent<SectorProxy>();
            lodFogSectorProxy._renderers = new List<Renderer> { lodMR };
            lodFogSectorProxy.SetSector(sector);
            */

            fogGO.transform.position = planetGO.transform.position;
            lodFogGO.transform.position = planetGO.transform.position;

            fogGO.SetActive(true);
            lodFogGO.SetActive(true);
        }
    }
}