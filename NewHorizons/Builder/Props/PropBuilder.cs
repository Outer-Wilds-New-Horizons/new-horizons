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
using OWML.Common;

namespace NewHorizons.Builder.Props
{
    public static class PropBuilder
    {
        public static void Make(GameObject go, Sector sector, IPlanetConfig config, IModAssets assets, string uniqueModName)
        {
            if (config.Props.Scatter != null) PropBuilder.Scatter(go, config.Props.Scatter, config.Base.SurfaceSize, sector);
            if(config.Props.Details != null)
            {
                foreach(var detail in config.Props.Details)
                {
                    if(detail.assetBundle != null)
                    {
                        string key = uniqueModName + "." + detail.assetBundle;
                        AssetBundle bundle;
                        GameObject prefab;

                        try
                        {
                            if (Main.AssetBundles.ContainsKey(key)) bundle = Main.AssetBundles[key];
                            else
                            {
                                bundle = assets.LoadBundle(detail.assetBundle);
                                Main.AssetBundles[key] = bundle;
                            }
                        }
                        catch(Exception e)
                        {
                            Logger.Log($"Couldn't load AssetBundle {detail.assetBundle} : {e.Message}");
                            return;
                        }

                        try
                        {
                            prefab = bundle.LoadAsset<GameObject>(detail.path);
                            prefab.SetActive(false);
                        }
                        catch(Exception e)
                        {
                            Logger.Log($"Couldn't load asset {detail.path} from AssetBundle {detail.assetBundle} : {e.Message}");
                            return;
                        }

                        MakeDetail(go, sector, prefab, detail.position, detail.rotation, detail.scale, detail.alignToNormal);
                    }
                    else if(detail.objFilePath != null)
                    {
                        try
                        {
                            var prefab = assets.Get3DObject(detail.objFilePath, detail.mtlFilePath);
                            prefab.SetActive(false);
                            MakeDetail(go, sector, prefab, detail.position, detail.rotation, detail.scale, detail.alignToNormal);
                        }
                        catch(Exception e)
                        {
                            Logger.LogError($"Could not load 3d object {detail.objFilePath} with texture {detail.mtlFilePath} : {e.Message}");
                        }
                    } 
                    else MakeDetail(go, sector, detail.path, detail.position, detail.rotation, detail.scale, detail.alignToNormal);
                }
            }
        }

        public static GameObject MakeDetail(GameObject go, Sector sector, string propToClone, MVector3 position, MVector3 rotation, float scale, bool alignWithNormal)
        {
            var prefab = GameObject.Find(propToClone);

            //TODO: this is super costly
            if (prefab == null) prefab = SearchUtilities.FindObjectOfTypeAndName<GameObject>(propToClone.Split(new char[] { '\\', '/' }).Last());
            if (prefab == null) Logger.LogError($"Couldn't find detail {propToClone}");
            return MakeDetail(go, sector, prefab, position, rotation, scale, alignWithNormal);
        }

        public static GameObject MakeDetail(GameObject go, Sector sector, GameObject prefab, MVector3 position, MVector3 rotation, float scale, bool alignWithNormal, bool snapToSurface = false)
        {
            if (prefab == null) return null;

            GameObject prop = GameObject.Instantiate(prefab, sector.transform);
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

            foreach (var component in prop.GetComponentsInChildren<Component>())
            {
                // TODO: Make this work or smthng
                if (component is GhostIK) (component as GhostIK).enabled = false;
                if(component is GhostEffects) (component as GhostEffects).enabled = false;
                

                var enabledField = component.GetType().GetField("enabled");
                if(enabledField != null && enabledField.FieldType == typeof(bool)) enabledField.SetValue(component, true);
            }

            prop.transform.parent = go.transform;
            prop.transform.localPosition = position == null ? Vector3.zero : (Vector3)position;

            Quaternion rot = rotation == null ? prefab.transform.rotation : Quaternion.Euler((Vector3)rotation);
            prop.transform.rotation = rot;
            if (alignWithNormal)
            {
                var up = prop.transform.localPosition.normalized;
                var front = Vector3.Cross(up, Vector3.left);
                if (front.sqrMagnitude == 0f) front = Vector3.Cross(up, Vector3.forward);
                if (front.sqrMagnitude == 0f) front = Vector3.Cross(up, Vector3.up);

                prop.transform.LookAt(prop.transform.position + front, up);
            }

            prop.transform.localScale = scale != 0 ? Vector3.one * scale : prefab.transform.localScale;

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
                    var prop = MakeDetail(go, sector, prefab, (MVector3)(point.normalized * radius), null, 0f, true, true);
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
