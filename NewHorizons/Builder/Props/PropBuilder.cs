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
            if (config.Props.Scatter != null)
            {
                PropBuilder.MakeScatter(go, config.Props.Scatter, config.Base.SurfaceSize, sector, assets, uniqueModName);
            }
            if(config.Props.Details != null)
            {
                foreach(var detail in config.Props.Details)
                {
                    if(detail.assetBundle != null)
                    {
                        var prefab = LoadPrefab(detail.assetBundle, detail.path, uniqueModName, assets);
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
            if(config.Props.Geysers != null)
            {
                foreach(var geyserInfo in config.Props.Geysers)
                {
                    GeyserBuilder.Make(go, sector, geyserInfo);
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

            foreach(var component in prop.GetComponents<SectoredMonoBehaviour>())
            {
                component.SetSector(sector);
                if(component is AnglerfishController)
                {
                    try
                    {
                        (component as AnglerfishController)._chaseSpeed += OWPhysics.CalculateOrbitVelocity(go.GetAttachedOWRigidbody(), go.GetComponent<AstroObject>().GetPrimaryBody().GetAttachedOWRigidbody()).magnitude;
                    }
                    catch(Exception e)
                    {
                        Logger.LogError($"Couldn't update AnglerFish chase speed: {e.Message}");
                    }
                }
            }
            foreach (var component in prop.GetComponentsInChildren<SectoredMonoBehaviour>())
            {
                component.SetSector(sector);
            }

            
            foreach (var component in prop.GetComponentsInChildren<Component>())
            {
                // TODO: Make this work or smthng
                if (component is GhostIK) (component as GhostIK).enabled = false;
                if(component is GhostEffects) (component as GhostEffects).enabled = false;

                var enabledField = component.GetType().GetField("enabled");
                if(enabledField != null && enabledField.FieldType == typeof(bool)) enabledField.SetValue(component, true);
            }
            

            prop.transform.position = position == null ? go.transform.position : go.transform.TransformPoint((Vector3)position);

            Quaternion rot = rotation == null ? Quaternion.identity : Quaternion.Euler((Vector3)rotation);
            prop.transform.localRotation = rot;
            if (alignWithNormal) prop.transform.AlignRadially();

            prop.transform.localScale = scale != 0 ? Vector3.one * scale : prefab.transform.localScale;

            prop.SetActive(true);

            return prop;
        }

        private static void MakeScatter(GameObject go, PropModule.ScatterInfo[] scatterInfo, float radius, Sector sector, IModAssets assets, string uniqueModName)
        {
            var area = 4f * Mathf.PI * radius * radius;
            var points = RandomUtility.FibonacciSphere((int)area);

            foreach (var propInfo in scatterInfo)
            {
                GameObject prefab;
                if (propInfo.assetBundle != null) prefab = LoadPrefab(propInfo.assetBundle, propInfo.path, uniqueModName, assets);
                else prefab = GameObject.Find(propInfo.path);
                for(int i = 0; i < propInfo.count; i++)
                {
                    var randomInd = (int)Random.Range(0, points.Count);
                    var point = points[randomInd];
                    var prop = MakeDetail(go, sector, prefab, (MVector3)(point.normalized * radius), null, propInfo.scale, true, true);
                    if(propInfo.offset != null) prop.transform.localPosition += prop.transform.TransformVector(propInfo.offset);
                    if(propInfo.rotation != null) prop.transform.rotation *= Quaternion.Euler(propInfo.rotation); 
                    points.RemoveAt(randomInd);
                    if (points.Count == 0) return;
                }
            }
        }

        private static GameObject LoadPrefab(string assetBundle, string path, string uniqueModName, IModAssets assets)
        {
            string key = uniqueModName + "." + assetBundle;
            AssetBundle bundle;
            GameObject prefab;

            try
            {
                if (Main.AssetBundles.ContainsKey(key)) bundle = Main.AssetBundles[key];
                else
                {
                    bundle = assets.LoadBundle(assetBundle);
                    Main.AssetBundles[key] = bundle;
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load AssetBundle {assetBundle} : {e.Message}");
                return null;
            }

            try
            {
                prefab = bundle.LoadAsset<GameObject>(path);
                prefab.SetActive(false);
            }
            catch (Exception e)
            {
                Logger.Log($"Couldn't load asset {path} from AssetBundle {assetBundle} : {e.Message}");
                return null;
            }

            return prefab;
        }
    }
}
