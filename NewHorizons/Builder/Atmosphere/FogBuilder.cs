using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using OWML.Common;
using System;
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

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            // Checking null here it was getting destroyed and wouldnt reload and never worked outside of the first loop
            // GetTexture caches itself anyway so it doesn't matter that this gets called multiple times
             _ramp = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/FogColorRamp.png");

            if (_isInit) return;

            _isInit = true;

            // Going to copy from dark bramble
            if (_lookupTexture == null) _lookupTexture = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogSphere_DB")?.GetComponent<PlanetaryFogController>()?.fogLookupTexture.DontDestroyOnLoad();
            if (_dbImpostorMesh == null) _dbImpostorMesh = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogLOD").GetComponent<MeshFilter>().mesh.DontDestroyOnLoad();
            if (_dbImpostorMaterials == null) _dbImpostorMaterials = SearchUtilities.Find("DarkBramble_Body/Atmosphere_DB/FogLOD").GetComponent<MeshRenderer>().sharedMaterials.MakePrefabMaterials();
        }

        #region obsolete
        // Never change method signatures, people directly reference the NH dll and it can break backwards compatibility
        // Dreamstalker needs this method signature
        [Obsolete]
        public static PlanetaryFogController Make(GameObject planetGO, Sector sector, AtmosphereModule atmo)
            => Make(planetGO, sector, atmo, null);
        #endregion

        public static PlanetaryFogController Make(GameObject planetGO, Sector sector, AtmosphereModule atmo, IModBehaviour mod)
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
            var colorRampTexture =
                atmo.fogRampPath != null ? ImageUtilities.GetTexture(mod, atmo.fogRampPath) :
                atmo.fogTint != null ? ImageUtilities.TintImage(_ramp, atmo.fogTint.ToColor()) :
                _ramp;

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
            // We apply fogTint to the material and tint the fog ramp, which means the ramp and tint get multiplied together in the shader, so tint is applied twice
            // However nobody has visually complained about this, so we don't want to change it until maybe somebody does
            // Was previously documented by issue #747.
            MR.material.SetTexture(ColorRampTexture, colorRampTexture);

            fogGO.transform.position = planetGO.transform.position;

            fogGO.SetActive(true);
            return PFC;
        }

        public static Renderer MakeProxy(GameObject proxyGO, AtmosphereModule atmo, IModBehaviour mod)
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

            var colorRampTexture =
                atmo.fogRampPath != null ? ImageUtilities.GetTexture(mod, atmo.fogRampPath) :
                atmo.fogTint != null ? ImageUtilities.TintImage(_ramp, atmo.fogTint.ToColor()) :
                _ramp;
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
