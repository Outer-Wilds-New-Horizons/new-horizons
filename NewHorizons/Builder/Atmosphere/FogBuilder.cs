using NewHorizons.External.Modules;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class FogBuilder
    {
        private static Texture2D _ramp;

        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmo)
        {
            if (_ramp == null) _ramp = ImageUtilities.GetTexture(Main.Instance, "AssetBundle/textures/FogColorRamp.png");

            GameObject fogGO = new GameObject("FogSphere");
            fogGO.SetActive(false);
            fogGO.transform.parent = sector?.transform ?? planetGO.transform;
            fogGO.transform.localScale = Vector3.one;

            // Going to copy from dark bramble
            var dbFog = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogLOD");
            var dbPlanetaryFogController = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogSphere_DB").GetComponent<PlanetaryFogController>();
            var brambleLODFog = SearchUtilities.Find("DarkBramble_Body/Sector_DB/Proxy_DB/LOD_DB_VolumeticFog");

            MeshFilter MF = fogGO.AddComponent<MeshFilter>();
            MF.mesh = dbFog.GetComponent<MeshFilter>().mesh;

            MeshRenderer MR = fogGO.AddComponent<MeshRenderer>();
            MR.materials = dbFog.GetComponent<MeshRenderer>().materials;
            MR.allowOcclusionWhenDynamic = true;

            PlanetaryFogController PFC = fogGO.AddComponent<PlanetaryFogController>();
            PFC.fogLookupTexture = dbPlanetaryFogController.fogLookupTexture;
            PFC.fogRadius = atmo.fogSize;
            PFC.fogDensity = atmo.fogDensity;
            PFC.fogExponent = 1f;
            PFC.fogColorRampTexture = atmo.fogTint == null ? _ramp : ImageUtilities.TintImage(_ramp, atmo.fogTint.ToColor());
            PFC.fogColorRampIntensity = 1f;
            PFC.fogTint = atmo.fogTint.ToColor();

            GameObject lodFogGO = new GameObject("LODFogSphere");
            lodFogGO.SetActive(false);
            lodFogGO.transform.parent = fogGO.transform;
            lodFogGO.transform.localScale = Vector3.one * atmo.size / 320f;

            MeshFilter lodMF = lodFogGO.AddComponent<MeshFilter>();
            lodMF.mesh = brambleLODFog.GetComponent<MeshFilter>().mesh;

            MeshRenderer lodMR = lodFogGO.AddComponent<MeshRenderer>();
            lodMR.material = new Material(brambleLODFog.GetComponent<MeshRenderer>().material);
            lodMR.material.color = atmo.fogTint.ToColor();
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
