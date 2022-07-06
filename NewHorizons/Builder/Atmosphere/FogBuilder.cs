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
            if (_ramp == null) _ramp = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/FogColorRamp.png");

            GameObject fogGO = new GameObject("FogSphere");
            fogGO.SetActive(false);
            fogGO.transform.parent = sector?.transform ?? planetGO.transform;
            fogGO.transform.localScale = Vector3.one;

            // Going to copy from dark bramble
            var dbFog = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogLOD");
            var dbPlanetaryFogController = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogSphere_DB").GetComponent<PlanetaryFogController>();

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

            fogGO.transform.position = planetGO.transform.position;

            fogGO.SetActive(true);
        }
    }
}
