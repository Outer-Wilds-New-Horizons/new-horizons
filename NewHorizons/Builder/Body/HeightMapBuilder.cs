using NewHorizons.Builder.Body.Geometry;
using NewHorizons.External.Modules;
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
                // tiles sample from this so we must wrap
                textureMap = Load(module.textureMap, "textureMap", true, false) ?? Texture2D.whiteTexture;
                smoothnessMap = Load(module.smoothnessMap, "smoothnessMap", false, false);
                normalMap = Load(module.normalMap, "normalMap", false, true);
                emissionMap = Load(module.emissionMap, "emissionMap", false, false);

                tileBlendMap = useLOD ? Load(module.tileBlendMap, "tileBlendMap", false, false) : null;

                baseTile = new Tile(useLOD ? module.baseTile : null, "BASE_TILE", "_BaseTile");
                redTile = new Tile(useLOD ? module.redTile : null, "RED_TILE", "_RedTile");
                greenTile = new Tile(useLOD ? module.greenTile : null, "GREEN_TILE", "_GreenTile");
                blueTile = new Tile(useLOD ? module.blueTile : null, "BLUE_TILE", "_BlueTile");
                alphaTile = new Tile(useLOD ? module.alphaTile : null, "ALPHA_TILE", "_AlphaTile");

                // Only delete heightmap if it hasn't been loaded yet
                deleteHeightmapFlag = !string.IsNullOrEmpty(module.heightMap) && !ImageUtilities.IsTextureLoaded(mod, module.heightMap);

                heightMap = Load(module.heightMap, "heightMap", false, false) ?? Texture2D.whiteTexture;
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
            cubeSphere.transform.SetParent(sector?.transform ?? planetGO.transform, false);

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

                var superGroup = planetGO.GetComponent<ProxyShadowCasterSuperGroup>();
                if (superGroup != null) level2.gameObject.AddComponent<ProxyShadowCaster>()._superGroup = superGroup;
            }

            var cubeSphereSC = cubeSphere.AddComponent<SphereCollider>();
            cubeSphereSC.radius = Mathf.Min(module.minHeight, module.maxHeight) * Mathf.Min(stretch.x, stretch.y, stretch.z);

            cubeSphere.SetActive(true);

            // Now that we've made the mesh we can delete the heightmap texture
            if (deleteHeightmapFlag) ImageUtilities.DeleteTexture(mod, module.heightMap, heightMap);

            return cubeSphere;

            MeshRenderer MakeLODTerrain(int resolution, bool useTriplanar)
            {
                var LODCubeSphere = new GameObject("LODCubeSphere");
                LODCubeSphere.transform.SetParent(cubeSphere.transform, false);

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

                if (useTriplanar)
                {
                    material.SetTexture("_BlendMap", tileBlendMap);

                    baseTile.TryApplyTile(material);
                    redTile.TryApplyTile(material);
                    greenTile.TryApplyTile(material);
                    blueTile.TryApplyTile(material);
                    alphaTile.TryApplyTile(material);
                }

                return cubeSphereMR;
            }
        }

        private static Texture2D Load(string path, string name, bool wrap, bool linear)
        {
            if (string.IsNullOrEmpty(path)) return null;

            if (!File.Exists(Path.Combine(_currentMod.ModHelper.Manifest.ModFolderPath, path)))
            {
                NHLogger.LogError($"Bad path for {_currentPlanetName} {name}: {path} couldn't be found.");
                return null;
            }

            return ImageUtilities.GetTexture(_currentMod, path, wrap: wrap, linear: linear);
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
                    _texture = Load(info.textureTile, $"{_prefix}TextureTile", true, false);
                    _smoothness = Load(info.smoothnessTile, $"{_prefix}SmoothnessTile", true, false);
                    _normal = Load(info.normalTile, $"{_prefix}NormalTile", true, true);
                }
                else
                {
                    // Visual studio won't compile if you don't do this idk
                    _texture = _smoothness = _normal = null;
                }
            }

            public void TryApplyTile(Material material)
            {
                if (_info != null)
                {
                    material.SetFloat(_prefix, 1);
                    material.EnableKeyword(_keyword);

                    material.SetFloat($"{_prefix}Scale", 1 / _info.size);
                    material.SetTexture($"{_prefix}Albedo", _texture);
                    material.SetTexture($"{_prefix}SmoothnessMap", _smoothness);
                    material.SetFloat($"{_prefix}BumpStrength", _info.normalStrength);
                    material.SetTexture($"{_prefix}BumpMap", _normal);
                }
            }
        }
    }
}
