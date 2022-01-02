using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using Logger = NewHorizons.Utility.Logger;
using System.Reflection;
using NewHorizons.Utility;

namespace NewHorizons.Builder.Props
{
    public static class PropBuilder
    {
        public static void Make(GameObject go, Sector sector, IPlanetConfig config)
        {
            if (config.Props.Scatter != null) PropBuilder.Scatter(go, config.Props.Scatter, config.Base.SurfaceSize, sector);
            if(config.Props.Details != null)
            {
                foreach(var detail in config.Props.Details)
                {
                    MakeDetail(go, sector, detail.path, detail.position, detail.rotation, detail.scale);
                }
            }
        }

        public static GameObject MakeDetail(GameObject go, Sector sector, string propToClone, MVector3 position, MVector3 rotation, float scale)
        {
            var prefab = GameObject.Find(propToClone);
            return MakeDetail(go, sector, prefab, position, rotation, scale);
        }

        public static GameObject MakeDetail(GameObject go, Sector sector, GameObject prefab, MVector3 position, MVector3 rotation, float scale, bool alignWithNormal = false)
        {
            if (prefab == null) return null;

            GameObject prop = GameObject.Instantiate(prefab, sector.transform);
            prop.transform.localPosition = position == null ? prefab.transform.localPosition : (Vector3)position;
            Quaternion rot = rotation == null ? prefab.transform.localRotation : Quaternion.Euler((Vector3)rotation);
            if (alignWithNormal) rot = Quaternion.FromToRotation(prop.transform.TransformDirection(Vector3.up), ((Vector3)position).normalized);
            prop.transform.rotation = rot;
            prop.transform.localScale = scale != 0 ? Vector3.one * scale : prefab.transform.localScale;
            prop.SetActive(false);

            List<string> assetBundles = new List<string>();
            foreach (var streamingHandle in prop.GetComponentsInChildren<StreamingMeshHandle>())
            {
                var assetBundle = streamingHandle.assetBundle;
                if (!assetBundles.Contains(assetBundle))
                {
                    assetBundles.Add(assetBundle);
                }
            }

            foreach (var assetBundle in assetBundles)
            {
                sector.OnOccupantEnterSector += ((SectorDetector sd) => StreamingManager.LoadStreamingAssets(assetBundle));
            }

            foreach(var component in prop.GetComponentsInChildren<Component>())
            {
                try
                {
                    var setSectorMethod = component.GetType().GetMethod("SetSector");
                    var sectorField = component.GetType().GetField("_sector");

                    if (setSectorMethod != null)
                    {
                        Logger.Log($"Found a SetSector method in {prop}.{component}");
                        setSectorMethod.Invoke(component, new object[] { sector });
                    }
                    else if (sectorField != null)
                    {
                        Logger.Log($"Found a _sector field in {component}");
                        sectorField.SetValue(component, sector);
                    }
                }
                catch (Exception e) { Logger.Log($"{e.Message}, {e.StackTrace}"); }
            }

            prop.SetActive(true);

            return prop;
        }

        private static void Scatter(GameObject go, PropModule.ScatterInfo[] scatterInfo, float radius, Sector sector)
        {
            var area = 4f * Mathf.PI * radius * radius;
            var points = FibonacciSphere((int)area);

            foreach (var propInfo in scatterInfo)
            {
                var prefab = GameObject.Find(propInfo.path);
                for(int i = 0; i < propInfo.count; i++)
                {
                    var randomInd = (int)Random.Range(0, points.Count);
                    var point = points[randomInd];
                    var prop = MakeDetail(go, sector, prefab, (MVector3)(point.normalized * radius), null, 0f);
                    if(propInfo.offset != null) prop.transform.localPosition += prop.transform.TransformVector(propInfo.offset);
                    if(propInfo.rotation != null) prop.transform.rotation *= Quaternion.Euler(propInfo.rotation); 
                    points.RemoveAt(randomInd);
                    if (points.Count == 0) return;
                }
            }
        }

        private static List<Vector3> FibonacciSphere(int samples)
        {
            List<Vector3> points = new List<Vector3>();

            var phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

            for(int i = 0; i < samples; i++)
            {
                var y = 1 - (i / (float)(samples - 1)) * 2f;
                var radius = Mathf.Sqrt(1 - y * y);

                var theta = phi * i;

                var x = Mathf.Cos(theta) * radius;
                var z = Mathf.Sin(theta) * radius;

                points.Add(new Vector3(x, y, z));
            }
            return points;
        }
    }
}
