using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
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

        public static void Make(GameObject go, Sector sector, IModBehaviour mod, PropModule.DetailInfo detail)
        {
            GameObject detailGO = null;

            if (detail.assetBundle != null)
            {
                var prefab = AssetBundleUtilities.LoadPrefab(detail.assetBundle, detail.path, mod);

                detailGO = MakeDetail(go, sector, prefab, detail);
            }
            else
            {
                var prefab = SearchUtilities.Find(detail.path);
                if (prefab == null) Logger.LogError($"Couldn't find detail {detail.path}");
                else detailGO = MakeDetail(go, sector, prefab, detail);
            }

            if (detailGO == null) return;

            if (detail.removeChildren != null)
            {
                var detailPath = detailGO.transform.GetPath();
                var transforms = detailGO.GetComponentsInChildren<Transform>(true);
                foreach (var childPath in detail.removeChildren)
                {
                    // Multiple children can have the same path so we delete all that match
                    var path = $"{detailPath}/{childPath}";

                    var flag = true;
                    foreach (var childObj in transforms.Where(x => x.GetPath() == path))
                    {
                        flag = false;
                        childObj.gameObject.SetActive(false);
                    }

                    if (flag) Logger.LogWarning($"Couldn't find \"{childPath}\".");
                }
            }

            if (detail.removeComponents)
            {
                // Just swap all the children to a new game object
                var newDetailGO = new GameObject(detailGO.name);
                newDetailGO.transform.position = detailGO.transform.position;
                newDetailGO.transform.parent = detailGO.transform.parent;
                // Can't modify parents while looping through children bc idk
                var children = new List<Transform>();
                foreach (Transform child in detailGO.transform)
                {
                    children.Add(child);
                }
                foreach (var child in children)
                {
                    child.parent = newDetailGO.transform;
                }
                GameObject.Destroy(detailGO);
                detailGO = newDetailGO;
            }

            if (detail.rename != null)
            {
                detailGO.name = detail.rename;
            }

            if (!string.IsNullOrEmpty(detail.parentPath))
            {
                var newParent = go.transform.Find(detail.parentPath);
                if (newParent != null)
                {
                    detailGO.transform.parent = newParent.transform;
                }
                else
                {
                    Logger.LogWarning($"Cannot find parent object at path: {go.name}/{detail.parentPath}");
                }
            }

            detailInfoToCorrespondingSpawnedGameObject[detail] = detailGO;
        }

        public static GameObject MakeDetail(GameObject planetGO, Sector sector, PropModule.DetailInfo info)
        {
            var prefab = SearchUtilities.Find(info.path);
            if (prefab == null)
            {
                Logger.LogError($"Couldn't find detail {info.path}");
                return null;
            }
            else
            {
                GameObject detailGO = MakeDetail(planetGO, sector, prefab, info);

                if (info.removeChildren != null)
                {
                    var detailPath = detailGO.transform.GetPath();
                    var transforms = detailGO.GetComponentsInChildren<Transform>(true);
                    foreach (var childPath in info.removeChildren)
                    {
                        // Multiple children can have the same path so we delete all that match
                        var path = $"{detailPath}/{childPath}";

                        var flag = true;
                        foreach (var childObj in transforms.Where(x => x.GetPath() == path))
                        {
                            flag = false;
                            childObj.gameObject.SetActive(false);
                        }

                        if (flag) Logger.LogWarning($"Couldn't find \"{childPath}\".");
                    }
                }

                if (info.removeComponents)
                {
                    // Just swap all the children to a new game object
                    var newDetailGO = new GameObject(detailGO.name);
                    newDetailGO.transform.position = detailGO.transform.position;
                    newDetailGO.transform.parent = detailGO.transform.parent;
                    // Can't modify parents while looping through children bc idk
                    var children = new List<Transform>();
                    foreach (Transform child in detailGO.transform)
                    {
                        children.Add(child);
                    }
                    foreach (var child in children)
                    {
                        child.parent = newDetailGO.transform;
                    }
                    GameObject.Destroy(detailGO);
                    detailGO = newDetailGO;
                }

                if (info.rename != null)
                {
                    detailGO.name = info.rename;
                }

                if (!string.IsNullOrEmpty(info.parentPath))
                {
                    var newParent = planetGO.transform.Find(info.parentPath);
                    if (newParent != null)
                    {
                        detailGO.transform.parent = newParent.transform;
                    }
                    else
                    {
                        Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                    }
                }

                return detailGO;
            }
        }

        public static GameObject MakeDetail(GameObject planetGO, Sector sector, GameObject prefab, PropModule.DetailInfo info)
        {
            if (prefab == null) return null;

            GameObject prop = prefab.InstantiateInactive();
            prop.name = prefab.name;
            prop.transform.parent = sector?.transform ?? planetGO.transform;

            StreamingHandler.SetUpStreaming(prop, sector);

            var isTorch = prop.GetComponent<VisionTorchItem>() != null;

            foreach (var component in prop.GetComponentsInChildren<Component>(true))
            {
                if (sector == null)
                {
                    if (FixUnsectoredComponent(component)) continue;
                }
                else FixSectoredComponent(component, sector, isTorch);

                FixComponent(component, planetGO, prefab.name);
            }

            prop.transform.position = info.position == null ? planetGO.transform.position : planetGO.transform.TransformPoint(info.position);

            Quaternion rot = info.rotation == null ? Quaternion.identity : Quaternion.Euler(info.rotation);

            if (info.alignToNormal)
            {
                // Apply the rotation after aligning it with normal
                var up = planetGO.transform.InverseTransformPoint(prop.transform.position).normalized;
                prop.transform.rotation = Quaternion.FromToRotation(Vector3.up, up);
                prop.transform.rotation *= rot;
            }
            else
            {
                prop.transform.rotation = planetGO.transform.TransformRotation(rot);
            }

            prop.transform.localScale = info.scale != 0 ? Vector3.one * info.scale : prefab.transform.localScale;

            prop.SetActive(true);

            return prop;
        }

        /// <summary>
        /// Fix components that have sectors. Has a specific fix if there is a VisionTorchItem on the object.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="sector"></param>
        /// <param name="isTorch"></param>
        private static void FixSectoredComponent(Component component, Sector sector, bool isTorch = false)
        {
            if (component is Sector s)
            {
                s.SetParentSector(sector);
            }

            if (component is SectorCullGroup sectorCullGroup)
            {
                sectorCullGroup._controllingProxy = null;
            }

            // fix Sector stuff, eg SectorCullGroup (without this, props that have a SectorCullGroup component will become invisible inappropriately)
            if (component is ISectorGroup sectorGroup)
            {
                sectorGroup.SetSector(sector);
            }

            if (component is SectoredMonoBehaviour behaviour)
            {
                behaviour.SetSector(sector);
            }

            if (component is OWItemSocket socket)
            {
                socket._sector = sector;
            }

            // Fix slide reel - Softlocks if this object is a vision torch
            if (!isTorch && component is SlideCollectionContainer container)
            {
                sector.OnOccupantEnterSector.AddListener(_ => container.LoadStreamingTextures());
            }

            if (component is NomaiRemoteCameraPlatform remoteCameraPlatform)
            {
                remoteCameraPlatform._visualSector = sector;
            }
        }

        /// <summary>
        /// Remove things that require sectors if the sector is null. Will just keep extending this as things pop up.
        /// Returns true if the object is destroyed
        /// </summary>
        private static bool FixUnsectoredComponent(Component component)
        {
            if (component is FogLight or SectoredMonoBehaviour)
            {
                GameObject.DestroyImmediate(component);
                return true;
            }
            return false;
        }

        private static void FixComponent(Component component, GameObject planetGO, string prefab)
        {
            // Fix other components
            // I forget why this is here
            if (component is GhostIK ik) ik.enabled = false;
            if (component is GhostEffects effects) effects.enabled = false;

            if (component is DarkMatterVolume)
            {
                var probeVisuals = component.gameObject.transform.Find("ProbeVisuals");
                if (probeVisuals != null) probeVisuals.gameObject.SetActive(true);
            }

            // Fix anglerfish speed on orbiting planets
            if (component is AnglerfishController angler)
            {
                try
                {
                    angler._chaseSpeed += OWPhysics.CalculateOrbitVelocity(planetGO.GetAttachedOWRigidbody(), planetGO.GetComponent<AstroObject>().GetPrimaryBody().GetAttachedOWRigidbody()).magnitude;
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't update AnglerFish chase speed:\n{e}");
                }
            }

            // fix campfires
            if (component is InteractVolume interactVolume)
            {
                interactVolume._playerCam = GameObject.Find("Player_Body/PlayerCamera").GetComponent<OWCamera>();
            }
            if (component is PlayerAttachPoint playerAttachPoint)
            {
                var playerBody = GameObject.Find("Player_Body");
                playerAttachPoint._playerController = playerBody.GetComponent<PlayerCharacterController>();
                playerAttachPoint._playerOWRigidbody = playerBody.GetComponent<OWRigidbody>();
                playerAttachPoint._playerTransform = playerBody.transform;
                playerAttachPoint._fpsCamController = GameObject.Find("Player_Body/PlayerCamera").GetComponent<PlayerCameraController>();
            }

            if (component is NomaiInterfaceOrb orb)
            {
                orb._parentAstroObject = planetGO.GetComponent<AstroObject>();
                orb._parentBody = planetGO.GetComponent<OWRigidbody>();
            }

            if (component is VisionTorchItem torchItem)
            {
                torchItem.enabled = true;
                torchItem.mindProjectorTrigger.enabled = true;
                torchItem.mindSlideProjector._mindProjectorImageEffect = SearchUtilities.Find("Player_Body/PlayerCamera").GetComponent<MindProjectorImageEffect>();
            }

            // Fix a bunch of stuff when done loading
            Delay.RunWhen(() => Main.IsSystemReady, () =>
            {
                try
                {
                    if (component == null) return;
                    if (component is Animator animator) animator.enabled = true;
                    else if (component is Collider collider) collider.enabled = true;
                    else if (component is Renderer renderer) renderer.enabled = true;
                    else if (component is Shape shape) shape.enabled = true;
                    else if (component is SectorCullGroup sectorCullGroup)
                    {
                        sectorCullGroup._inMapView = false;
                        sectorCullGroup._isFastForwarding = false;
                        sectorCullGroup.SetVisible(sectorCullGroup.ShouldBeVisible(), true, false);
                    }
                    // If it's not a moving anglerfish make sure the anim controller is regular
                    else if (component is AnglerfishAnimController angler && angler.GetComponentInParent<AnglerfishController>() == null)
                    {
                        Logger.LogVerbose("Enabling anglerfish animation");
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
                }
                catch (Exception e)
                {
                    Logger.LogWarning($"Exception when modifying component [{component.GetType().Name}] on [{planetGO.name}] for prop [{prefab}]:\n{e}");
                }
            });
        }
    }
}