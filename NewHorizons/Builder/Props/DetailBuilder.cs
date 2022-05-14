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
using NewHorizons.Handlers;

namespace NewHorizons.Builder.Props
{
    public static class DetailBuilder
    {
        private static Dictionary<PropModule.DetailInfo, GameObject> detailInfoToCorrespondingSpawnedGameObject = new Dictionary<PropModule.DetailInfo, GameObject>();

        public static GameObject GetSpawnedGameObjectByDetailInfo(PropModule.DetailInfo detail)
        {
            if (!detailInfoToCorrespondingSpawnedGameObject.ContainsKey(detail)) return null;
            return detailInfoToCorrespondingSpawnedGameObject[detail];
        }

        public static void Make(GameObject go, Sector sector, IPlanetConfig config, IModBehaviour mod, string uniqueModName, PropModule.DetailInfo detail)
        {
            GameObject detailGO = null;

            if (detail.assetBundle != null)
            {
                var prefab = PropBuildManager.LoadPrefab(detail.assetBundle, detail.path, uniqueModName, mod);

                detailGO = MakeDetail(go, sector, prefab, detail.position, detail.rotation, detail.scale, detail.alignToNormal);
            }
            else if (detail.objFilePath != null)
            {
                try
                {
                    var prefab = mod.ModHelper.Assets.Get3DObject(detail.objFilePath, detail.mtlFilePath);
                    PropBuildManager.ReplaceShaders(prefab);
                    prefab.SetActive(false);
                    detailGO = MakeDetail(go, sector, prefab, detail.position, detail.rotation, detail.scale, detail.alignToNormal);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Could not load 3d object {detail.objFilePath} with texture {detail.mtlFilePath} : {e.Message}");
                }
            }
            else detailGO = MakeDetail(go, sector, detail.path, detail.position, detail.rotation, detail.scale, detail.alignToNormal);

            if(detailGO != null && detail.removeChildren != null)
            {
                foreach(var childPath in detail.removeChildren)
                {
                    var childObj = detailGO.transform.Find(childPath);
                    if (childObj != null) childObj.gameObject.SetActive(false);
                    else Logger.LogWarning($"Couldn't find {childPath}");
                }
            }

            detailInfoToCorrespondingSpawnedGameObject[detail] = detailGO;
        }

        public static GameObject MakeDetail(GameObject go, Sector sector, string propToClone, MVector3 position, MVector3 rotation, float scale, bool alignWithNormal)
        {
            var prefab = SearchUtilities.Find(propToClone);
            if (prefab == null) Logger.LogError($"Couldn't find detail {propToClone}");
            return MakeDetail(go, sector, prefab, position, rotation, scale, alignWithNormal);
        }

        public static GameObject MakeDetail(GameObject planetGO, Sector sector, GameObject prefab, MVector3 position, MVector3 rotation, float scale, bool alignWithNormal)
        {
            if (prefab == null) return null;

            GameObject prop = GameObject.Instantiate(prefab, sector.transform);
            prop.SetActive(false);

            sector.OnOccupantEnterSector += (SectorDetector sd) => OWAssetHandler.OnOccupantEnterSector(prop, sd, sector);
            OWAssetHandler.LoadObject(prop);

            foreach (var component in prop.GetComponents<Component>().Concat(prop.GetComponentsInChildren<Component>()))
            {
                // Enable all children or something
                var enabledField = component?.GetType()?.GetField("enabled");
                if (enabledField != null && enabledField.FieldType == typeof(bool)) Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => enabledField.SetValue(component, true));

                if(component is Sector)
                {
                    (component as Sector)._parentSector = sector;
                }

                // TODO: Make this work or smthng
                if (component is GhostIK) (component as GhostIK).enabled = false;
                if (component is GhostEffects) (component as GhostEffects).enabled = false;

                if(component is DarkMatterVolume)
                {
                    var probeVisuals = component.gameObject.transform.Find("ProbeVisuals");
                    if (probeVisuals != null) probeVisuals.gameObject.SetActive(true);
                }

                if (component is SectoredMonoBehaviour)
                {
                    (component as SectoredMonoBehaviour).SetSector(sector);
                }
                else
                {
                    var sectorField = component?.GetType()?.GetField("_sector");
                    if (sectorField != null && sectorField.FieldType == typeof(Sector)) Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => sectorField.SetValue(component, sector));
                }

                if (component is AnglerfishController)
                {
                    try
                    {
                        (component as AnglerfishController)._chaseSpeed += OWPhysics.CalculateOrbitVelocity(planetGO.GetAttachedOWRigidbody(), planetGO.GetComponent<AstroObject>().GetPrimaryBody().GetAttachedOWRigidbody()).magnitude;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Couldn't update AnglerFish chase speed: {e.Message}");
                    }
                }

                // Fix slide reel
                if(component is SlideCollectionContainer)
                {
                    sector.OnOccupantEnterSector.AddListener((_) => (component as SlideCollectionContainer).LoadStreamingTextures());
                }

                if(component is OWItemSocket)
                {
                    (component as OWItemSocket)._sector = sector;
                }

                // Fix a bunch of stuff when done loading
                Main.Instance.ModHelper.Events.Unity.RunWhen(() => Main.IsSystemReady, () =>
                {
                    if (component is Animator) (component as Animator).enabled = true;
                    else if (component is Collider) (component as Collider).enabled = true;
                    else if (component is Renderer) (component as Renderer).enabled = true;
                    else if (component is Shape) (component as Shape).enabled = true;
                    // If it's not a moving anglerfish make sure the anim controller is regular
                    else if (component is AnglerfishAnimController && component.GetComponentInParent<AnglerfishController>() == null)
                    {
                        Logger.Log("Enabling anglerfish animation");
                        var angler = (component as AnglerfishAnimController);
                        // Remove any reference to its angler
                        if (angler._anglerfishController)
                        {
                            angler._anglerfishController.OnChangeAnglerState -= angler.OnChangeAnglerState;
                            angler._anglerfishController.OnAnglerTurn -= angler.OnAnglerTurn;
                            angler._anglerfishController.OnAnglerSuspended -= angler.OnAnglerSuspended;
                            angler._anglerfishController.OnAnglerUnsuspended -= angler.OnAnglerUnsuspended;
                        }
                        angler.enabled = true;
                        angler.OnChangeAnglerState(AnglerfishController.AnglerState.Lurking);
                    }
                });
            }

            prop.transform.position = position == null ? planetGO.transform.position : planetGO.transform.TransformPoint((Vector3)position);

            Quaternion rot = rotation == null ? Quaternion.identity : Quaternion.Euler((Vector3)rotation);
            prop.transform.rotation = planetGO.transform.TransformRotation(Quaternion.identity);
            if (alignWithNormal)
            {
                // Apply the rotation after aligning it with normal
                var up = planetGO.transform.InverseTransformPoint(prop.transform.position).normalized;
                prop.transform.rotation = Quaternion.FromToRotation(prop.transform.up, up) * prop.transform.rotation;
                prop.transform.rotation *= rot;
            }
            else
            {
                prop.transform.rotation = planetGO.transform.TransformRotation(rot);
            }

            prop.transform.localScale = scale != 0 ? Vector3.one * scale : prefab.transform.localScale;

            prop.SetActive(true);

            return prop;
        }
    }
}
