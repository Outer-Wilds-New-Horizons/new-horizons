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

        private static Texture3D _lookupTexture;
        private static Mesh _dbImpostorMesh;
        private static Material[] _dbImpostorMaterials;

        internal static void InitPrefabs()
        {
            if (_ramp == null) _ramp = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/FogColorRamp.png");

            // Going to copy from dark bramble
            if (_lookupTexture == null) _lookupTexture = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogSphere_DB")?.GetComponent<PlanetaryFogController>()?.fogLookupTexture.DontDestroyOnLoad();
            if (_dbImpostorMesh == null) _dbImpostorMesh = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogLOD").GetComponent<MeshFilter>().mesh.DontDestroyOnLoad();
            if (_dbImpostorMaterials == null) _dbImpostorMaterials = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogLOD").GetComponent<MeshRenderer>().sharedMaterials.MakePrefabMaterials();
        }

        public static PlanetaryFogController Make(GameObject planetGO, Sector sector, AtmosphereModule atmo)
        {
            InitPrefabs();

            GameObject fogGO = new GameObject("FogSphere");
            fogGO.SetActive(false);
            fogGO.transform.parent = sector?.transform ?? planetGO.transform;
            fogGO.transform.localScale = Vector3.one * atmo.fogSize;

            MeshFilter MF = fogGO.AddComponent<MeshFilter>();
            MF.mesh = _dbImpostorMesh;

            MeshRenderer MR = fogGO.AddComponent<MeshRenderer>();
            MR.materials = _dbImpostorMaterials;
            MR.allowOcclusionWhenDynamic = true;

            PlanetaryFogController PFC = fogGO.AddComponent<PlanetaryFogController>();
            PFC._fogImpostor = MR;
            PFC.fogLookupTexture = _lookupTexture;
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
            return PFC;
        }

        public static Renderer MakeProxy(GameObject proxyGO, AtmosphereModule atmo)
        {
            InitPrefabs();

            GameObject fogGO = new GameObject("FogSphere");
            fogGO.SetActive(false);
            fogGO.transform.parent = proxyGO.transform;
            fogGO.transform.localScale = Vector3.one * atmo.fogSize;

            MeshFilter MF = fogGO.AddComponent<MeshFilter>();
            MF.mesh = _dbImpostorMesh;

            MeshRenderer MR = fogGO.AddComponent<MeshRenderer>();
            MR.materials = _dbImpostorMaterials;
            MR.allowOcclusionWhenDynamic = true;

            var colorRampTexture = atmo.fogTint == null ? _ramp : ImageUtilities.TintImage(_ramp, atmo.fogTint.ToColor());
            if (atmo.fogTint != null)
            {
                MR.material.SetColor(Tint, atmo.fogTint.ToColor());
            }
            MR.material.SetFloat(Radius, atmo.fogSize);
            MR.material.SetFloat(Density, atmo.fogDensity);
            MR.material.SetFloat(DensityExponent, 1);
            MR.material.SetTexture(ColorRampTexture, colorRampTexture);

            fogGO.transform.position = proxyGO.transform.position;

            fogGO.SetActive(true);
            return MR;
        }
    }
}
