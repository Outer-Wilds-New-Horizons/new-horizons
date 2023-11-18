using NewHorizons.External;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.SerializableData;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.Geometry;
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
        public const string FIBONACCI_SPHERE_CACHE_KEY = "ScatterBuilderPoints";

        public static void Make(GameObject go, Sector sector, NewHorizonsBody body)
        {
            MakeScatter(go, body.Config.Props.scatter, body.Config.Base.surfaceSize, sector, body);
        }

        private static void MakeScatter(GameObject go, ScatterInfo[] scatterInfo, float radius, Sector sector, NewHorizonsBody body)
        {
            var heightMap = body.Config.HeightMap;
            var deleteHeightmapFlag = false;

            List<Vector3> points = new();

            if (body.Cache.ContainsKey(FIBONACCI_SPHERE_CACHE_KEY))
            {
                points = body.Cache.Get<ScatterCacheData>(FIBONACCI_SPHERE_CACHE_KEY).points.Select(x => (Vector3)x).ToList();
            }
            else if (scatterInfo.Any(x => x.preventOverlap))
            {
                var area = 4f * Mathf.PI * radius * radius;

                // To not use more than 0.5GB of RAM while doing this 
                // Works up to planets with 575 radius before capping
                var numPoints = Math.Min((int)(area * 10), 41666666);

                // This method is really slow and inefficient says nebula
                // So now we cache it
                points = RandomUtility.FibonacciSphere(numPoints);

                body.Cache.Set(FIBONACCI_SPHERE_CACHE_KEY, new ScatterCacheData() { points = points.Select(x => (MVector3)x).ToArray() });
            }

            Texture2D heightMapTexture = null;
            if (heightMap != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(heightMap.heightMap))
                    {
                        deleteHeightmapFlag = !ImageUtilities.IsTextureLoaded(body.Mod, heightMap.heightMap);
                        heightMapTexture = ImageUtilities.GetTexture(body.Mod, heightMap.heightMap);
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
                if (body.Config.Water != null && propInfo.minHeight == null)
                {
                    propInfo.minHeight = body.Config.Water.size - 1f;
                }

                GameObject prefab;
                if (propInfo.assetBundle != null)
                {
                    prefab = AssetBundleUtilities.LoadPrefab(propInfo.assetBundle, propInfo.path, body.Mod);
                }
                else
                {
                    prefab = SearchUtilities.Find(propInfo.path);
                }

                // Run all the make detail stuff on it early and just copy it over and over instead
                var detailInfo = new DetailInfo()
                {
                    scale = propInfo.scale,
                    stretch = propInfo.stretch,
                    keepLoaded = propInfo.keepLoaded
                };
                var scatterPrefab = DetailBuilder.Make(go, sector, prefab, detailInfo);

                for (int i = 0; i < propInfo.count; i++)
                {
                    Vector3 point;
                    // Use our generated list of points if we need to prevent overlap
                    if (propInfo.preventOverlap)
                    {
                        if (points.Count > 0)
                        {
                            var randomInd = Random.Range(0, points.Count - 1);
                            point = points[randomInd];
                            points.QuickRemoveAt(randomInd);
                        }
                        else
                        {
                            // They must be scattering a ton of points
                            // This shouldn't happen
                            break;
                        }
                    }
                    // Else take a random point
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

                    var prop = scatterPrefab.InstantiateInactive();
                    prop.transform.SetParent(sector?.transform ?? go.transform);
                    prop.transform.localPosition = go.transform.TransformPoint(point * height);
                    var up = go.transform.InverseTransformPoint(prop.transform.position).normalized;
                    prop.transform.rotation = Quaternion.FromToRotation(Vector3.up, up);

                    if (propInfo.offset != null)
                    {
                        prop.transform.localPosition += prop.transform.TransformVector(propInfo.offset);
                    }
                    if (propInfo.rotation != null)
                    {
                        prop.transform.rotation *= Quaternion.Euler(propInfo.rotation);
                    }

                    // Rotate around normal
                    prop.transform.localRotation *= Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);

                    prop.SetActive(true);
                }

                Object.Destroy(scatterPrefab);

                if (deleteHeightmapFlag && heightMapTexture != null)
                {
                    ImageUtilities.DeleteTexture(body.Mod, heightMap.heightMap, heightMapTexture);
                }
            }
        }

        [Serializable]
        private struct ScatterCacheData
        {
            public MVector3[] points;
        }
    }
}
