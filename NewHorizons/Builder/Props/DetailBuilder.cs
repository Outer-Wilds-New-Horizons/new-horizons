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
    public static class DetailBuilder
    {
        public static void Make(GameObject go, Sector sector, IPlanetConfig config, IModAssets assets, string uniqueModName, PropModule.DetailInfo detail)
        {
            if (detail.assetBundle != null)
            {
                var prefab = PropBuildManager.LoadPrefab(detail.assetBundle, detail.path, uniqueModName, assets);
                MakeDetail(go, sector, prefab, detail.position, detail.rotation, detail.scale, detail.alignToNormal, detail.generateColliders);
            }
            else if (detail.objFilePath != null)
            {
                try
                {
                    var prefab = assets.Get3DObject(detail.objFilePath, detail.mtlFilePath);
                    prefab.SetActive(false);
                    MakeDetail(go, sector, prefab, detail.position, detail.rotation, detail.scale, detail.alignToNormal, detail.generateColliders);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Could not load 3d object {detail.objFilePath} with texture {detail.mtlFilePath} : {e.Message}");
                }
            }
            else MakeDetail(go, sector, detail.path, detail.position, detail.rotation, detail.scale, detail.alignToNormal, detail.generateColliders);
        }

        public static GameObject MakeDetail(GameObject go, Sector sector, string propToClone, MVector3 position, MVector3 rotation, float scale, bool alignWithNormal, bool generateColliders)
        {
            var prefab = GameObject.Find(propToClone);

            //TODO: this is super costly
            if (prefab == null) prefab = SearchUtilities.FindObjectOfTypeAndName<GameObject>(propToClone.Split(new char[] { '\\', '/' }).Last());
            if (prefab == null) Logger.LogError($"Couldn't find detail {propToClone}");
            return MakeDetail(go, sector, prefab, position, rotation, scale, alignWithNormal, generateColliders);
        }

        public static GameObject MakeDetail(GameObject go, Sector sector, GameObject prefab, MVector3 position, MVector3 rotation, float scale, bool alignWithNormal, bool generateColliders)
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
                StreamingManager.LoadStreamingAssets(assetBundle);
            }

            foreach (var component in prop.GetComponents<Component>().Concat(prop.GetComponentsInChildren<Component>()))
            {
                // Enable all children or something
                var enabledField = component.GetType().GetField("enabled");
                if (enabledField != null && enabledField.FieldType == typeof(bool)) enabledField.SetValue(component, true);

                // TODO: Make this work or smthng
                if (component is GhostIK) (component as GhostIK).enabled = false;
                if (component is GhostEffects) (component as GhostEffects).enabled = false;

                if (component is Animator) Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => (component as Animator).enabled = true);
                if (component is Collider) Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => (component as Collider).enabled = true);

                if (component is SectoredMonoBehaviour)
                {
                    (component as SectoredMonoBehaviour).SetSector(sector);
                }

                if (component is AnglerfishController)
                {
                    try
                    {
                        (component as AnglerfishController)._chaseSpeed += OWPhysics.CalculateOrbitVelocity(go.GetAttachedOWRigidbody(), go.GetComponent<AstroObject>().GetPrimaryBody().GetAttachedOWRigidbody()).magnitude;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Couldn't update AnglerFish chase speed: {e.Message}");
                    }
                }

                // Mesh colliders
                if (generateColliders)
                {
                    if (component is MeshFilter && component.gameObject.GetComponent<MeshCollider>() == null)
                    {
                        var mesh = (component as MeshFilter).mesh;
                        if (mesh.isReadable) component.gameObject.AddComponent<MeshCollider>();
                        else Logger.LogError($"Couldn't change mesh for {component.gameObject.name} because it is not readable");
                    }
                }
            }

            prop.transform.position = position == null ? go.transform.position : go.transform.TransformPoint((Vector3)position);

            Quaternion rot = rotation == null ? Quaternion.identity : Quaternion.Euler((Vector3)rotation);
            prop.transform.localRotation = rot;
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
    }
}
