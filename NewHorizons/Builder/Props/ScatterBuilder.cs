using NewHorizons.External.Configs;
using NewHorizons.External.Modules.Props;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.Geometry;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private static void MakeScatter(GameObject go, ScatterInfo[] scatterInfo, float radius, Sector sector, IModBehaviour mod, PlanetConfig config)
        {
            var heightMap = config.HeightMap;
            var deleteHeightmapFlag = false;

            var makeFibonacciSphere = scatterInfo.Any(x => x.preventOverlap);

            List<Vector3> points = new();

            if (makeFibonacciSphere)
            {
                var area = 4f * Mathf.PI * radius * radius;

                // To not use more than 0.5GB of RAM while doing this 
                // Works up to planets with 575 radius before capping
                var numPoints = Math.Min((int)(area * 10), 41666666);

                points = RandomUtility.FibonacciSphere(numPoints);
            }

            Texture2D heightMapTexture = null;
            if (heightMap != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(heightMap.heightMap))
                    {
                        deleteHeightmapFlag = !ImageUtilities.IsTextureLoaded(mod, heightMap.heightMap);
                        heightMapTexture = ImageUtilities.GetTexture(mod, heightMap.heightMap);
                    }
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

                // By default don't put underwater more than a meter
                // this is a backward compat thing lol
                if (config.Water != null && propInfo.minHeight == null) propInfo.minHeight = config.Water.size - 1f;

                GameObject prefab;
                if (propInfo.assetBundle != null) prefab = AssetBundleUtilities.LoadPrefab(propInfo.assetBundle, propInfo.path, mod);
                else prefab = SearchUtilities.Find(propInfo.path);

                // Run all the make detail stuff on it early and just copy it over and over instead
                var detailInfo = new DetailInfo()
                {
                    scale = propInfo.scale,
                    stretch = propInfo.stretch,
					keepLoaded = propInfo.keepLoaded,
                };
                var scatterPrefab = DetailBuilder.Make(go, ref sector, mod, prefab, detailInfo);

                for (int i = 0; i < propInfo.count; i++)
                {
                    Vector3 point;
                    if (propInfo.preventOverlap) 
                    {
                        if (points.Count == 0) break;
                        var randomInd = Random.Range(0, points.Count - 1);
                        point = points[randomInd];
                        points.QuickRemoveAt(randomInd);
                    }
                    else
                    {
                        point = Random.onUnitSphere;
                    }

                    var height = radius;
                    if (heightMapTexture != null)
                    {
                        var sphericals = CoordinateUtilities.CartesianToSpherical(point, true);
                        float longitude = sphericals.x;
                        float latitude = sphericals.y;

                        float sampleX = heightMapTexture.width * longitude / 360f;

                        // Fix wrapping issue
                        if (sampleX > heightMapTexture.width) sampleX -= heightMapTexture.width;
                        if (sampleX < 0) sampleX += heightMapTexture.width;

                        float sampleY = heightMapTexture.height * latitude / 180f;

                        float relativeHeight = heightMapTexture.GetPixel((int)sampleX, (int)sampleY).r;
                        height = (relativeHeight * (heightMap.maxHeight - heightMap.minHeight) + heightMap.minHeight);

                        if ((propInfo.minHeight != null && height < propInfo.minHeight) || (propInfo.maxHeight != null && height > propInfo.maxHeight))
                        {
                            // Try this point again
                            i--;
                            continue;
                        }
                    }


                    var parent = sector?.transform ?? go.transform;

                    if (go != null && !string.IsNullOrEmpty(propInfo.parentPath))
                    {
                        var newParent = go.transform.Find(propInfo.parentPath);
                        if (newParent != null)
                        {
                            parent = newParent;
                            sector = newParent.GetComponentInParent<Sector>();
                        }
                        else
                        {
                            NHLogger.LogError($"Cannot find parent object at path: {go.name}/{propInfo.parentPath}");
                        }
                    }

                    var prop = scatterPrefab.InstantiateInactive();
                    // Have to use SetParent method to work with tidally locked bodies #872
                    prop.transform.SetParent(parent, false);
                    prop.transform.localPosition = point * height;
                    var up = (prop.transform.position - go.transform.position).normalized;
                    prop.transform.rotation = Quaternion.FromToRotation(Vector3.up, up);

                    if (propInfo.offset != null) prop.transform.localPosition += prop.transform.TransformVector(propInfo.offset);
                    if (propInfo.rotation != null) prop.transform.rotation *= Quaternion.Euler(propInfo.rotation);

                    // Rotate around normal
                    prop.transform.localRotation *= Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);

                    prop.SetActive(true);
                }

                Object.Destroy(scatterPrefab);

                if (deleteHeightmapFlag && heightMapTexture != null)
                {
                    ImageUtilities.DeleteTexture(mod, heightMap.heightMap, heightMapTexture);
                }
            }
        }
    }
}
