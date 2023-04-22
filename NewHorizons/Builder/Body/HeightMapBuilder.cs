using NewHorizons.Builder.Body.Geometry;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.IO;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public static class HeightMapBuilder
    {
        public static Shader PlanetShader;

        // I hate nested functions okay
        private static IModBehaviour _currentMod;
        private static string _currentPlanetName;

        public static GameObject Make(GameObject planetGO, Sector sector, HeightMapModule module, IModBehaviour mod, int resolution, bool useLOD = false)
        {
            bool deleteHeightmapFlag;

            Texture2D heightMap, textureMap, smoothnessMap, normalMap, emissionMap, tileBlendMap;

            Tile baseTile, redTile, greenTile, blueTile, alphaTile;

            _currentMod = mod;
            _currentPlanetName = planetGO.name;

            try
            {
                textureMap = Load(module.textureMap, "textureMap", false) ?? Texture2D.whiteTexture;
                smoothnessMap = Load(module.smoothnessMap, "smoothnessMap", false);
                normalMap = Load(module.normalMap, "normalMap", true);
                emissionMap = Load(module.emissionMap, "emissionMap", false);

                baseTile = new Tile(module.baseTile, "BASE_TILE", "_BaseTile");
                redTile = new Tile(module.baseTile, "RED_TILE", "_RedTile");
                greenTile = new Tile(module.baseTile, "GREEN_TILE", "_GreenTile");
                blueTile = new Tile(module.baseTile, "BLUE_TILE", "_BlueTile");
                alphaTile = new Tile(module.baseTile, "ALPHA_TILE", "_AlphaTile");

                tileBlendMap = useLOD ? Load(module.tileBlendMap, "tileBlendMap", false) : null;

                // Only delete heightmap if it hasn't been loaded yet
                deleteHeightmapFlag = !string.IsNullOrEmpty(module.heightMap) && !ImageUtilities.IsTextureLoaded(mod, module.heightMap);

                heightMap = Load(module.heightMap, "heightMap", false) ?? Texture2D.whiteTexture;
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Couldn't load HeightMap textures:\n{e}");
                return null;
            }

            _currentMod = null;
            _currentPlanetName = null;

            var cubeSphere = new GameObject("CubeSphere");
            cubeSphere.SetActive(false);
            cubeSphere.transform.parent = sector?.transform ?? planetGO.transform;

            if (PlanetShader == null) PlanetShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/SphereTextureWrapperTriplanar.shader");

            var stretch = module.stretch != null ? (Vector3)module.stretch : Vector3.one;

            var emissionColor = module.emissionColor?.ToColor() ?? Color.white;

            var level1 = MakeLODTerrain(resolution, useLOD);

            var cubeSphereMC = cubeSphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = level1.gameObject.GetComponent<MeshFilter>().mesh;

            if (useLOD)
            {
                var level2Res = (int)Mathf.Clamp(resolution / 2f, 1 /*cube moment*/, 100);
                var level2 = MakeLODTerrain(level2Res, false);

                var LODGroup = cubeSphere.AddComponent<LODGroup>();
                LODGroup.size = module.maxHeight;

                LODGroup.SetLODs(new LOD[]
                {
                    new LOD(1 / 3f, new Renderer[] { level1 }),
                    new LOD(0, new Renderer[] { level2 })
                });

                level1.name += "0";
                level2.name += "1";

                LODGroup.RecalculateBounds();
            }

            var cubeSphereSC = cubeSphere.AddComponent<SphereCollider>();
            cubeSphereSC.radius = Mathf.Min(module.minHeight, module.maxHeight) * Mathf.Min(stretch.x, stretch.y, stretch.z);

            var superGroup = planetGO.GetComponent<ProxyShadowCasterSuperGroup>();
            if (superGroup != null) cubeSphere.AddComponent<ProxyShadowCaster>()._superGroup = superGroup;

            // rotate for backwards compat :P
            cubeSphere.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(0, -90, 0));
            cubeSphere.transform.position = planetGO.transform.position;

            cubeSphere.SetActive(true);

            // Now that we've made the mesh we can delete the heightmap texture
            if (deleteHeightmapFlag) ImageUtilities.DeleteTexture(mod, module.heightMap, heightMap);

            return cubeSphere;

            MeshRenderer MakeLODTerrain(int resolution, bool useTriplanar)
            {
                var LODCubeSphere = new GameObject("LODCubeSphere");

                LODCubeSphere.AddComponent<MeshFilter>().mesh = CubeSphere.Build(resolution, heightMap, module.minHeight, module.maxHeight, stretch);

                var cubeSphereMR = LODCubeSphere.AddComponent<MeshRenderer>();
                var material = new Material(PlanetShader);
                cubeSphereMR.material = material;
                material.name = textureMap.name;

                material.mainTexture = textureMap;
                material.SetFloat("_Smoothness", module.smoothness);
                material.SetFloat("_Metallic", module.metallic);
                material.SetTexture("_SmoothnessMap", smoothnessMap);
                material.SetFloat("_BumpStrength", module.normalStrength);
                material.SetTexture("_BumpMap", normalMap);
                material.SetColor("_EmissionColor", emissionColor);
                material.SetTexture("_EmissionMap", emissionMap);

                if (useTriplanar) material.SetTexture("_BlendMap", tileBlendMap);

                baseTile.TryApplyTile(material, useTriplanar);
                redTile.TryApplyTile(material, useTriplanar);
                greenTile.TryApplyTile(material, useTriplanar);
                blueTile.TryApplyTile(material, useTriplanar);
                alphaTile.TryApplyTile(material, useTriplanar);

                LODCubeSphere.transform.parent = cubeSphere.transform;
                LODCubeSphere.transform.localPosition = Vector3.zero;

                return cubeSphereMR;
            }
        }

        private static Texture2D Load(string path, string name, bool linear)
        {
            if (string.IsNullOrEmpty(path)) return null;

            if (!File.Exists(Path.Combine(_currentMod.ModHelper.Manifest.ModFolderPath, path)))
            {
                NHLogger.LogError($"Bad path for {_currentPlanetName} {name}: {path} couldn't be found.");
                return null;
            }

            return ImageUtilities.GetTexture(_currentMod, path, wrap: true, linear: linear);
        }

        private readonly struct Tile
        {
            private readonly HeightMapModule.HeightMapTileInfo _info;
            private readonly string _keyword, _prefix;
            private readonly Texture2D _texture, _smoothness, _normal;

            public Tile(HeightMapModule.HeightMapTileInfo info, string keyword, string prefix)
            {
                _info = info;

                _keyword = keyword;
                _prefix = prefix;

                if (_info != null)
                {
                    _texture = Load(info.textureTile, $"{_prefix}TextureTile", false);
                    _smoothness = Load(info.smoothnessTile, $"{_prefix}SmoothnessTile", false);
                    _normal = Load(info.normalTile, $"{_prefix}NormalTile", false);
                }
            }

            public void TryApplyTile(Material material, bool applyTriplanar)
            {
                if (applyTriplanar && _info != null)
                {
                    material.EnableKeyword(_keyword);
                    material.SetFloat($"{_prefix}Scale", 1 / _info.size);
                    material.SetTexture($"{_prefix}Albedo", _texture);
                    material.SetTexture($"{_prefix}SmoothnessMap", _smoothness);
                    material.SetFloat($"{_prefix}BumpStrength", _info.normalStrength);
                    material.SetTexture($"{_prefix}BumpMap", _normal);
                }
                else
                {
                    // This might just be disabled by default which would simplify a few things here but nobody wants to check (me included)
                    material.DisableKeyword(_keyword);
                }
            }
        }
    }
}
