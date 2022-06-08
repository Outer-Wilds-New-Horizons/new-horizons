using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using OWML.Common;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
namespace NewHorizons.Builder.Props
{
    public static class ScatterBuilder
    {
        public static void Make(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod)
        {
            MakeScatter(go, config.Props.scatter, config.Base.surfaceSize, sector, mod, config);
        }

        private static void MakeScatter(GameObject go, PropModule.ScatterInfo[] scatterInfo, float radius, Sector sector, IModBehaviour mod, PlanetConfig config)
        {
            var heightMap = config.HeightMap;

            var area = 4f * Mathf.PI * radius * radius;
            var points = RandomUtility.FibonacciSphere((int)(area * 10));

            Texture2D heightMapTexture = null;
            if (heightMap != null)
            {
                try
                {
                    heightMapTexture = ImageUtilities.GetTexture(mod, heightMap.heightMap);
                    // defer remove texture to next frame
                    Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => Object.Destroy(heightMapTexture));
                }
                catch (Exception) { }
                if (heightMapTexture == null)
                {
                    radius = heightMap.maxHeight;
                }
            }

            foreach (var propInfo in scatterInfo)
            {
                Random.InitState(propInfo.seed);

                GameObject prefab;
                if (propInfo.assetBundle != null) prefab = AssetBundleUtilities.LoadPrefab(propInfo.assetBundle, propInfo.path, mod);
                else prefab = SearchUtilities.Find(propInfo.path);
                for (int i = 0; i < propInfo.count; i++)
                {
                    var randomInd = (int)Random.Range(0, points.Count - 1);
                    var point = points[randomInd];

                    var height = radius;
                    if (heightMapTexture != null)
                    {
                        var sphericals = CoordinateUtilities.CartesianToSpherical(point);
                        float longitude = sphericals.x;
                        float latitude = sphericals.y;

                        float sampleX = heightMapTexture.width * longitude / 360f;
                        float sampleY = heightMapTexture.height * latitude / 180f;

                        float relativeHeight = heightMapTexture.GetPixel((int)sampleX, (int)sampleY).r;
                        height = (relativeHeight * (heightMap.maxHeight - heightMap.minHeight) + heightMap.minHeight);

                        // Because heightmaps are dumb gotta rotate it 90 degrees around the x axis bc UHHHHHHHHHHHHH
                        point = Quaternion.Euler(90, 0, 0) * point;

                        // Keep things mostly above water
                        if (config.Water != null && height - 1f < config.Water.size) continue;

                        // Move it slightly into the ground
                        height -= 0.1f;
                    }

                    var prop = DetailBuilder.MakeDetail(go, sector, prefab, (MVector3)(point.normalized * height), null, propInfo.scale, true);
                    if (propInfo.offset != null) prop.transform.localPosition += prop.transform.TransformVector(propInfo.offset);
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
