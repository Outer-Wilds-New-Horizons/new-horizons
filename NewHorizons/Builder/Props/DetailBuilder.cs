using NewHorizons.Builder.General;
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

        public static void RegisterDetailInfo(PropModule.DetailInfo detail, GameObject detailGO)
        {
            detailInfoToCorrespondingSpawnedGameObject[detail] = detailGO;
        }

        /// <summary>
        /// Create a detail using an asset bundle or a path in the scene hierarchy of the item to copy.
        /// </summary>
        public static GameObject Make(GameObject go, Sector sector, IModBehaviour mod, PropModule.DetailInfo detail)
        {
            if (detail.assetBundle != null)
            {
                // Shouldn't happen
                if (mod == null) return null;

                return Make(go, sector, AssetBundleUtilities.LoadPrefab(detail.assetBundle, detail.path, mod), detail);
            }
            else
                return Make(go, sector, detail);
        }

        /// <summary>
        /// Create a detail using a path in the scene hierarchy of the item to copy.
        /// </summary>
        public static GameObject Make(GameObject planetGO, Sector sector, PropModule.DetailInfo info)
        {
            var prefab = SearchUtilities.Find(info.path);
            if (prefab == null)
            {
                Logger.LogError($"Couldn't find detail {info.path}");
                return null;
            }
            else
                return Make(planetGO, sector, prefab, info);
        }

        /// <summary>
        /// Create a detail using a prefab.
        /// </summary>
        public static GameObject Make(GameObject go, Sector sector, GameObject prefab, PropModule.DetailInfo detail)
        {
            if (prefab == null) return null;

            GameObject prop = prefab.InstantiateInactive();
            prop.name = prefab.name;
            prop.transform.parent = sector?.transform ?? go.transform;

            StreamingHandler.SetUpStreaming(prop, sector);

            var isTorch = prop.GetComponent<VisionTorchItem>() != null;

            foreach (var component in prop.GetComponentsInChildren<Component>(true))
            {
                if (sector == null)
                {
                    if (FixUnsectoredComponent(component)) continue;
                }
                else FixSectoredComponent(component, sector, isTorch);

                FixComponent(component, go, prefab.name);
            }

            prop.transform.position = detail.position == null ? go.transform.position : go.transform.TransformPoint(detail.position);

            Quaternion rot = detail.rotation == null ? Quaternion.identity : Quaternion.Euler(detail.rotation);

            if (detail.alignToNormal)
            {
                // Apply the rotation after aligning it with normal
                var up = go.transform.InverseTransformPoint(prop.transform.position).normalized;
                prop.transform.rotation = Quaternion.FromToRotation(Vector3.up, up);
                prop.transform.rotation *= rot;
            }
            else
            {
                prop.transform.rotation = go.transform.TransformRotation(rot);
            }

            prop.transform.localScale = detail.scale != 0 ? Vector3.one * detail.scale : prefab.transform.localScale;

            if (!detail.keepLoaded) GroupBuilder.Make(prop, sector);
            prop.SetActive(true);

            if (prop == null) return null;

            if (detail.removeChildren != null)
            {
                var detailPath = prop.transform.GetPath();
                var transforms = prop.GetComponentsInChildren<Transform>(true);
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
                var newDetailGO = new GameObject(prop.name);
                newDetailGO.transform.position = prop.transform.position;
                newDetailGO.transform.parent = prop.transform.parent;

                // Can't modify parents while looping through children bc idk
                var children = new List<Transform>();
                foreach (Transform child in prop.transform)
                {
                    children.Add(child);
                }
                foreach (var child in children)
                {
                    child.parent = newDetailGO.transform;
                }
                GameObject.Destroy(prop);
                prop = newDetailGO;
            }

            if (detail.rename != null)
            {
                prop.name = detail.rename;
            }

            if (!string.IsNullOrEmpty(detail.parentPath))
            {
                var newParent = go.transform.Find(detail.parentPath);
                if (newParent != null)
                {
                    prop.transform.parent = newParent.transform;
                }
            }

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
            if (component is GhostIK || component is GhostEffects)
            {
                Component.DestroyImmediate(component);
                return;
            }

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