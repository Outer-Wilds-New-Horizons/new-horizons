using NewHorizons.External.Modules;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class FogBuilder
    {
        private static Texture2D _ramp;

        private static readonly int FogTexture = Shader.PropertyToID("_FogTex");
        private static readonly int Tint = Shader.PropertyToID("_Tint");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int Density = Shader.PropertyToID("_Density");
        private static readonly int DensityExponent = Shader.PropertyToID("_DensityExp");
        private static readonly int ColorRampTexture = Shader.PropertyToID("_ColorRampTex");

        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmo)
        {
            if (_ramp == null) _ramp = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/FogColorRamp.png");

            GameObject fogGO = new GameObject("FogSphere");
            fogGO.SetActive(false);
            fogGO.transform.parent = sector?.transform ?? planetGO.transform;
            fogGO.transform.localScale = Vector3.one * atmo.fogSize;

            // Going to copy from dark bramble
            var dbFog = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogLOD");
            var dbPlanetaryFogController = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogSphere_DB").GetComponent<PlanetaryFogController>();

            MeshFilter MF = fogGO.AddComponent<MeshFilter>();
            MF.mesh = dbFog.GetComponent<MeshFilter>().mesh;

            MeshRenderer MR = fogGO.AddComponent<MeshRenderer>();
            MR.materials = dbFog.GetComponent<MeshRenderer>().materials;
            MR.allowOcclusionWhenDynamic = true;

            PlanetaryFogController PFC = fogGO.AddComponent<PlanetaryFogController>();
            PFC._fogImpostor = MR;
            PFC.fogLookupTexture = dbPlanetaryFogController.fogLookupTexture;
            PFC.fogRadius = atmo.fogSize;
            PFC.lodFadeDistance = PFC.fogRadius * 0.5f;
            PFC.fogDensity = atmo.fogDensity;
            PFC.fogExponent = 1f;
            var colorRampTexture = atmo.fogTint == null ? _ramp : ImageUtilities.TintImage(_ramp, atmo.fogTint.ToColor());
            PFC.fogColorRampTexture = colorRampTexture;
            PFC.fogColorRampIntensity = 1f;
            if (atmo.fogTint != null)
            {
                PFC.fogTint = atmo.fogTint.ToColor();

                MR.material.SetColor(Tint, atmo.fogTint.ToColor());
            }
            MR.material.SetFloat(Radius, atmo.fogSize);
            MR.material.SetFloat(Density, atmo.fogDensity);
            MR.material.SetFloat(DensityExponent, 1);
            MR.material.SetTexture(ColorRampTexture, colorRampTexture);

            fogGO.transform.position = planetGO.transform.position;

            fogGO.SetActive(true);
        }
    }
}
