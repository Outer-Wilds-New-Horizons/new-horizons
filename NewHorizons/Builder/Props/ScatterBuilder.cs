using NewHorizons.External;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.SerializableData;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.Geometry;
using Newtonsoft.Json;
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
        public const string FIBONACCI_SPHERE_CACHE_KEY_PREFIX = "ScatterBuilderPoints";

        public static void Make(GameObject go, Sector sector, NewHorizonsBody body)
        {
            MakeScatter(go, body.Config.Props.scatter, body.Config.Base.surfaceSize, sector, body);
        }

        // I hate this but see no better way to clear the remaining heightmap texture
        private static Texture2D _heightmapTextureToClear;

        private static Dictionary<string, MVector3[]> GetScatterPoints(NewHorizonsBody body, ScatterInfo[] scatterInfo, float radius)
        {
            // The key contains the radius and scatter info values since thats what decides the points list ultimately
            // NHCache clears unused values so if they change the radius the old points list gets removed
            var cacheKey = FIBONACCI_SPHERE_CACHE_KEY_PREFIX + radius + string.Join(",", scatterInfo.Select(x => JsonConvert.SerializeObject(x).GetHashCode()));

            if (body.Cache.ContainsKey(cacheKey))
            {
                return body.Cache.Get<ScatterCacheData>(cacheKey).points;
            }

            // Load the heightmap for the planet, this will be used to position the scattered props
            var heightMap = body.Config.HeightMap;
            Texture2D heightMapTexture = null;
            if (heightMap != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(heightMap.heightMap))
                    {
                        var deleteHeightmapFlag = !ImageUtilities.IsTextureLoaded(body.Mod, heightMap.heightMap);
                        heightMapTexture = ImageUtilities.GetTexture(body.Mod, heightMap.heightMap);

                        if (deleteHeightmapFlag)
                        {
                            _heightmapTextureToClear = heightMapTexture;
                        }
                    }
                }
                catch (Exception) { }
                // If the image cant be loaded but they have a heightmap for some reason use the max height
                // This is what the heighmap module does when it fails to load the heightmap texture (makes a texuted sphere using maxheight as radius)
                if (heightMapTexture == null)
                {
                    radius = heightMap.maxHeight;
                }
            }

            // Number of points is proportional to the radius of the sphere
            var area = 4f * Mathf.PI * radius * radius;

            // To not use more than 0.5GB of RAM while doing this 
            // Works up to planets with 575 radius before capping
            var numPoints = Math.Min((int)(area * 10), 41666666);

            // This method is really slow and inefficient says nebula
            // Since we now cache the resulting points it should be fine
            var fibonacciPoints = scatterInfo.Any(x => x.preventOverlap) ? RandomUtility.FibonacciSphere(numPoints) : null;

            var points = new Dictionary<string, MVector3[]>();
            foreach (var propInfo in scatterInfo)
            {
                var propPositions = new MVector3[propInfo.count];

                // By default don't put underwater more than a meter
                // this is a backward compat thing lol
                if (body.Config.Water != null && propInfo.minHeight == null)
                {
                    propInfo.minHeight = body.Config.Water.size - 1f;
                }

                Random.InitState(propInfo.seed);
                for (int i = 0; i < propInfo.count; i++)
                {
                    var point = Random.onUnitSphere;
                    if (propInfo.preventOverlap && points.Count > 0) 
                    { 
                        var randomInd = Random.Range(0, fibonacciPoints.Count - 1);
                        point = fibonacciPoints[randomInd];
                        fibonacciPoints.QuickRemoveAt(randomInd);
                    }

                    // Check vs our heightmap to see if this point is a valid position
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

                    propPositions[i] = point * height;
                }

                points[propInfo.path] = propPositions;
            }

            body.Cache.Set(cacheKey, new ScatterCacheData() { points = points });

            return points;
        }

        private static GameObject CreateScatterPrefab(GameObject go, Sector sector, ScatterInfo propInfo, NewHorizonsBody body)
        {
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

            return scatterPrefab;
        }

        private static void MakeScatter(GameObject go, ScatterInfo[] scatterInfo, float radius, Sector sector, NewHorizonsBody body)
        {
            var points = GetScatterPoints(body, scatterInfo, radius);

            foreach (var propInfo in scatterInfo)
            {
                Random.InitState(propInfo.seed);

                var scatterPrefab = CreateScatterPrefab(go, sector, propInfo, body);

                var propPositions = points[propInfo.path];

                for (int i = 0; i < propInfo.count; i++)
                {
                    var point = (Vector3)propPositions[i];
                    CreateScatteredProp(go, sector, propInfo, scatterPrefab, point);
                }

                Object.Destroy(scatterPrefab);
            }

            if (_heightmapTextureToClear != null)
            {
                ImageUtilities.DeleteTexture(body.Mod, body.Config.HeightMap.heightMap, _heightmapTextureToClear);
            }
        }

        private static void CreateScatteredProp(GameObject go, Sector sector, ScatterInfo propInfo, GameObject scatterPrefab, Vector3 point)
        {
            var prop = scatterPrefab.InstantiateInactive();
            prop.transform.SetParent(sector?.transform ?? go.transform);
            prop.transform.localPosition = go.transform.TransformPoint(point);
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

        [Serializable]
        private struct ScatterCacheData
        {
            public Dictionary<string, MVector3[]> points;
        }
    }
}
