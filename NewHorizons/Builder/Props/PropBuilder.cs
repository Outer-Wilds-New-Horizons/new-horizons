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

namespace NewHorizons.Builder.Props
{
    public static class PropBuilder
    {
        public static GameObject Make(GameObject body, string propToClone, Vector3 position, Sector sector)
        {
            var prefab = GameObject.Find(propToClone);
            return Make(body, prefab, position, sector);
        }

        public static GameObject Make(GameObject body, GameObject prefab, Vector3 position, Sector sector)
        {
            if (prefab == null) return null;

            GameObject prop = GameObject.Instantiate(prefab, sector.transform);
            prop.transform.localPosition = position;
            prop.transform.rotation = Quaternion.FromToRotation(prop.transform.TransformDirection(Vector3.up), position.normalized);
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

        public static void Scatter(GameObject body, PropModule.ScatterInfo[] scatterInfo, float radius, Sector sector)
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
                    var prop = Make(body, prefab, point.normalized * radius, sector);
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
