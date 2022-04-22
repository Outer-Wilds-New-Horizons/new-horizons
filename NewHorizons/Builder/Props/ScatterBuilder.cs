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
using NewHorizons.External.Configs;

namespace NewHorizons.Builder.Props
{
    public static class ScatterBuilder
    {
        public static void Make(GameObject go, Sector sector, IPlanetConfig config, IModBehaviour mod, string uniqueModName)
        {
            MakeScatter(go, config.Props.Scatter, config.Base.SurfaceSize, sector, mod, uniqueModName, config);
        }

        private static void MakeScatter(GameObject go, PropModule.ScatterInfo[] scatterInfo, float radius, Sector sector, IModBehaviour mod, string uniqueModName, IPlanetConfig config)
        {
            var heightMap = config.HeightMap;

            var area = 4f * Mathf.PI * radius * radius;
            var points = RandomUtility.FibonacciSphere(Math.Max((int)(area * 10), );

            Texture2D heightMapTexture = null;
            if (heightMap != null)
            {
                try
                {
                    heightMapTexture = ImageUtilities.GetTexture(mod, heightMap.HeightMap);
                }
                catch (Exception) { }
                if(heightMapTexture == null)
                {
                    radius = heightMap.MaxHeight;
                }
            }

            foreach (var propInfo in scatterInfo)
            {
                Random.InitState(propInfo.seed);

                GameObject prefab;
                if (propInfo.assetBundle != null) prefab = PropBuildManager.LoadPrefab(propInfo.assetBundle, propInfo.path, uniqueModName, mod);
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
                        height -= 0.1f;
                    }

                    var prop = DetailBuilder.MakeDetail(go, sector, prefab, (MVector3)(point.normalized * height), null, propInfo.scale, true);
                    if (propInfo.offset != null) prop.transform.localPosition += prop.transform.TransformVector(propInfo.offset);
                    if (propInfo.rotation != null) prop.transform.rotation *= Quaternion.Euler(propInfo.rotation);

                    // Rotate around normal
                    prop.transform.localRotation *= Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);

                    points.RemoveAt(randomInd % points.Count);
                    if (points.Count == 0) return;
                }
            }
        }
    }
}
