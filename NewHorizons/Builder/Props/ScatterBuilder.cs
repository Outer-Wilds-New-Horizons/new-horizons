#region

using System;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace NewHorizons.Builder.Props
{
    public static class ScatterBuilder
    {
        public static void Make(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod,
            string uniqueModName)
        {
            MakeScatter(go, config.Props.scatter, config.Base.surfaceSize, sector, mod, uniqueModName, config);
        }

        private static void MakeScatter(GameObject go, PropModule.ScatterInfo[] scatterInfo, float radius,
            Sector sector, IModBehaviour mod, string uniqueModName, PlanetConfig config)
        {
            var heightMap = config.HeightMap;

            var area = 4f * Mathf.PI * radius * radius;
            var points = RandomUtility.FibonacciSphere((int) (area * 10));

            Texture2D heightMapTexture = null;
            if (heightMap != null)
            {
                try
                {
                    heightMapTexture = ImageUtilities.GetTexture(mod, heightMap.heightMap);
                }
                catch (Exception)
                {
                }

                if (heightMapTexture == null) radius = heightMap.maxHeight;
            }

            foreach (var propInfo in scatterInfo)
            {
                Random.InitState(propInfo.seed);

                GameObject prefab;
                if (propInfo.assetBundle != null)
                    prefab = AssetBundleUtilities.LoadPrefab(propInfo.assetBundle, propInfo.path, mod);
                else prefab = GameObject.Find(propInfo.path);
                for (var i = 0; i < propInfo.count; i++)
                {
                    var randomInd = Random.Range(0, points.Count - 1);
                    var point = points[randomInd];

                    var height = radius;
                    if (heightMapTexture != null)
                    {
                        var sphericals = CoordinateUtilities.CartesianToSpherical(point);
                        var longitude = sphericals.x;
                        var latitude = sphericals.y;

                        var sampleX = heightMapTexture.width * longitude / 360f;
                        var sampleY = heightMapTexture.height * latitude / 180f;

                        var relativeHeight = heightMapTexture.GetPixel((int) sampleX, (int) sampleY).r;
                        height = relativeHeight * (heightMap.maxHeight - heightMap.minHeight) + heightMap.minHeight;

                        // Because heightmaps are dumb gotta rotate it 90 degrees around the x axis bc UHHHHHHHHHHHHH
                        point = Quaternion.Euler(90, 0, 0) * point;

                        // Keep things mostly above water
                        if (config.Water != null && height - 1f < config.Water.size) continue;

                        // Move it slightly into the ground
                        height -= 0.1f;
                    }

                    var prop = DetailBuilder.MakeDetail(go, sector, prefab, point.normalized * height, null,
                        propInfo.scale, true);
                    if (propInfo.offset != null)
                        prop.transform.localPosition += prop.transform.TransformVector(propInfo.offset);
                    if (propInfo.rotation != null) prop.transform.rotation *= Quaternion.Euler(propInfo.rotation);

                    // Rotate around normal
                    prop.transform.localRotation *= Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);

                    points.RemoveAt(randomInd);
                    if (points.Count == 0) return;
                }
            }
        }
    }
}