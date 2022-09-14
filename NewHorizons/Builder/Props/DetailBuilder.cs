using NewHorizons.Builder.General;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using Component = UnityEngine.Component;
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

            // Fix a bunch of stuff when done loading
            var fixes = new List<Component>();

            foreach (var component in prop.GetComponentsInChildren<Component>(true))
            {
                if (sector == null)
                {
                    if (FixUnsectoredComponent(component)) continue;
                }
                else FixSectoredComponent(component, sector, isTorch);

                FixComponent(component, go, prefab.name);

                if (DetailFixer.fixes.Keys.Any(x => x.IsAssignableFrom(component.GetType())))
                {
                    fixes.Add(component);
                }
            }

            if (fixes.Count > 0)
            {
                prop.AddComponent<DetailFixer>().SetFixes(fixes);
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

            if (!detail.keepLoaded) GroupsBuilder.Make(prop, sector);
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
        }

        /// <summary>
        /// Performs fixes that have to be done after the system loads
        /// Has to be done this way to ensure that scatter works
        /// </summary>
        private class DetailFixer : MonoBehaviour
        {
            public static Dictionary<Type, Action<Component>> fixes = new()
            {
                [typeof(AnglerfishAnimController)] = (x) =>
                {
                    var angler = x as AnglerfishAnimController;

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
                },
                [typeof(SectorCullGroup)] = (x) =>
                {
                    var sectorCullGroup = x as SectorCullGroup;
                    sectorCullGroup._inMapView = false;
                    sectorCullGroup._isFastForwarding = false;
                    sectorCullGroup.SetVisible(sectorCullGroup.ShouldBeVisible(), true, false);
                },
                [typeof(Shape)] = (x) => (x as Shape).enabled = true,
                [typeof(Renderer)] = (x) => (x as Renderer).enabled = true,
                [typeof(Collider)] = (x) => (x as Collider).enabled = true,
                [typeof(Animator)] = (x) => (x as Animator).enabled = true
            };   

            // Have to be public to be copied by Instantiate
            public Component[] componentsToFix;

            public void SetFixes(List<Component> fixes)
            {
                // Components must be in a list for unity to properly deep copy
                componentsToFix = fixes.ToArray();
            }

            public void Start()
            {
                for (int i = 0; i < componentsToFix.Length; i++)
                {
                    var component = componentsToFix[i];

                    try
                    {
                        if (component != null)
                        {
                            var key = fixes.Keys.FirstOrDefault(x => x.IsAssignableFrom(component.GetType()));
                            var fix = fixes[key];
                            fix(component);
                        }
                    }
                    catch (Exception)
                    {
                        Logger.LogWarning($"Failed to fix component {component} on {gameObject.name}");
                    }
                }
            }
        }
    }
}