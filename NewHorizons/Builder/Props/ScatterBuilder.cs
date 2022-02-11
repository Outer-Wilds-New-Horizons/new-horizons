using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.External;
using OWML.Common;

namespace NewHorizons.Builder.Props
{
    public static class ScatterBuilder
    {
        public static void Make(GameObject go, Sector sector, IPlanetConfig config, IModAssets assets, string uniqueModName)
        {
            MakeScatter(go, config.Props.Scatter, config.Base.SurfaceSize, sector, assets, uniqueModName, config);
        }

        private static void MakeScatter(GameObject go, PropModule.ScatterInfo[] scatterInfo, float radius, Sector sector, IModAssets assets, string uniqueModName, IPlanetConfig config)
        {
            var heightMap = config.HeightMap;

            var area = 4f * Mathf.PI * radius * radius;
            var points = RandomUtility.FibonacciSphere((int)(area * 10));

            Texture2D heightMapTexture = null;
            if (heightMap != null)
            {
                try
                {
                    heightMapTexture = assets.GetTexture(heightMap.HeightMap);
                }
                catch (Exception) { }
            }

            foreach (var propInfo in scatterInfo)
            {
                Random.InitState(propInfo.seed);

                GameObject prefab;
                if (propInfo.assetBundle != null) prefab = PropBuildManager.LoadPrefab(propInfo.assetBundle, propInfo.path, uniqueModName, assets);
                else prefab = GameObject.Find(propInfo.path);
                for (int i = 0; i < propInfo.count; i++)
                {
                    var randomInd = (int)Random.Range(0, points.Count);
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
                        height = (relativeHeight * (heightMap.MaxHeight - heightMap.MinHeight) + heightMap.MinHeight);

                        // Because heightmaps are dumb gotta rotate it 90 degrees around the x axis bc UHHHHHHHHHHHHH
                        point = Quaternion.Euler(90, 0, 0) * point;

                        // Keep things mostly above water
                        if (config.Water != null && height - 1f < config.Water.Size) continue;

                        // Move it slightly into the ground
                        height -= 0.2f;
                    }

                    var prop = DetailBuilder.MakeDetail(go, sector, prefab, (MVector3)(point.normalized * height), null, propInfo.scale, true);
                    if (propInfo.offset != null) prop.transform.localPosition += prop.transform.TransformVector(propInfo.offset);
                    if (propInfo.rotation != null) prop.transform.rotation *= Quaternion.Euler(propInfo.rotation);
                    points.RemoveAt(randomInd);
                    if (points.Count == 0) return;
                }
            }
        }
    }
}
