using NewHorizons.Builder.General;
using NewHorizons.Components;
using NewHorizons.Components.Fixers;
using NewHorizons.External.Modules.Props;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewHorizons.Builder.Props
{
    public static class DetailBuilder
    {
        private static readonly Dictionary<DetailInfo, GameObject> _detailInfoToCorrespondingSpawnedGameObject = new();
        private static readonly Dictionary<(Sector, string), (GameObject prefab, bool isItem)> _fixedPrefabCache = new();

        static DetailBuilder()
        {
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private static void SceneManager_sceneUnloaded(Scene scene)
        {
            foreach (var prefab in _fixedPrefabCache.Values)
            {
                UnityEngine.Object.Destroy(prefab.prefab);
            }
            _fixedPrefabCache.Clear();
            _detailInfoToCorrespondingSpawnedGameObject.Clear();
        }

        public static GameObject GetSpawnedGameObjectByDetailInfo(DetailInfo detail)
        {
            if (!_detailInfoToCorrespondingSpawnedGameObject.ContainsKey(detail))
            {
                return null;
            }
            else
            {
                return _detailInfoToCorrespondingSpawnedGameObject[detail];
            }
        }

        /// <summary>
        /// Create a detail using an asset bundle or a path in the scene hierarchy of the item to copy.
        /// </summary>
        public static GameObject Make(GameObject go, Sector sector, IModBehaviour mod, DetailInfo detail)
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
        public static GameObject Make(GameObject planetGO, Sector sector, DetailInfo info)
        {
            var prefab = SearchUtilities.Find(info.path);
            if (prefab == null)
            {
                NHLogger.LogError($"Couldn't find detail {info.path}");
                return null;
            }
            else
                return Make(planetGO, sector, prefab, info);
        }

        /// <summary>
        /// Create a detail using a prefab.
        /// </summary>
        public static GameObject Make(GameObject go, Sector sector, GameObject prefab, DetailInfo detail)
        {
            if (prefab == null) return null;

            GameObject prop;
            bool isItem;
            bool invalidComponentFound = false;

            // We save copies with all their components fixed, good if the user is placing the same detail more than once
            if (detail?.path != null && _fixedPrefabCache.TryGetValue((sector, detail.path), out var storedPrefab))
            {
                prop = GeneralPropBuilder.MakeFromPrefab(storedPrefab.prefab, prefab.name, go, sector, detail);
                isItem = storedPrefab.isItem;
            }
            else
            {
                prop = GeneralPropBuilder.MakeFromPrefab(prefab, prefab.name, go, sector, detail);

                StreamingHandler.SetUpStreaming(prop, detail.keepLoaded ? null : sector);

                // Could check this in the for loop but I'm not sure what order we need to know about this in
                var isTorch = prop.GetComponent<VisionTorchItem>() != null;
                isItem = false;

                foreach (var component in prop.GetComponentsInChildren<Component>(true))
                {
                    // Components can come through as null here (yes, really),
                    // Usually if a script was added to a prefab in an asset bundle but isn't present in the loaded mod DLLs
                    if (component == null)
                    {
                        invalidComponentFound = true;
                        continue;
                    }
                    if (component.gameObject == prop && component is OWItem) isItem = true;

                    if (sector == null)
                    {
                        if (FixUnsectoredComponent(component)) continue;
                    }
                    else FixSectoredComponent(component, sector, isTorch, detail.keepLoaded);

                    FixComponent(component, go, detail.ignoreSun);
                }

                if (detail.path != null)
                {
                    // We put these in DontDestroyOnLoad so that QSB will ignore them and so they don't clutter up the scene.
                    _fixedPrefabCache.Add((sector, detail.path), (prop.InstantiateInactive().DontDestroyOnLoad(), isItem));
                }
            }

            if (invalidComponentFound)
            {
                foreach (Transform t in prop.GetComponentsInChildren<Transform>(true))
                {
                    if (t.GetComponents<Component>().Any(c => c == null))
                    {
                        NHLogger.LogError($"Failed to instantiate component at {t.GetPath()}. This usually means there's a missing script.");
                    }
                }
            }

            // Items shouldn't use these else they get weird
            if (isItem) detail.keepLoaded = true;

            prop.transform.localScale = detail.stretch ?? (detail.scale != 0 ? Vector3.one * detail.scale : prefab.transform.localScale);

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

                    if (flag) NHLogger.LogWarning($"Couldn't find \"{childPath}\".");
                }
            }

            if (detail.removeComponents)
            {
                NHLogger.LogVerbose($"Removing all components from [{prop.name}]");

                // Just swap all the children to a new game object
                var newDetailGO = new GameObject(prop.name);
                newDetailGO.SetActive(false);
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
                // Have to destroy it right away, else parented props might get attached to the old one
                UnityEngine.Object.DestroyImmediate(prop);
                prop = newDetailGO;
            }
            
            if (isItem)
            {
                // Else when you put them down you can't pick them back up
                var col = prop.GetComponent<OWCollider>();
                if (col != null) col._physicsRemoved = false;
            }

            if (!detail.keepLoaded) GroupsBuilder.Make(prop, sector);
            prop.SetActive(true);

            if (detail.hasPhysics)
            {
                var addPhysics = prop.AddComponent<AddPhysics>();
                addPhysics.Sector = sector;
                addPhysics.Mass = detail.physicsMass;
                addPhysics.Radius = detail.physicsRadius;
            }

            _detailInfoToCorrespondingSpawnedGameObject[detail] = prop;

            return prop;
        }

        /// <summary>
        /// Fix components that have sectors. Has a specific fix if there is a VisionTorchItem on the object.
        /// </summary>
        private static void FixSectoredComponent(Component component, Sector sector, bool isTorch, bool keepLoaded)
        {
            // keepLoaded should remove existing groups
            // renderers/colliders get enabled later so we dont have to do that here
            if (keepLoaded && component is SectorCullGroup or SectorCollisionGroup or SectorLightsCullGroup)
            {
                UnityEngine.Object.DestroyImmediate(component);
                return;
            }

            // fix Sector stuff, eg SectorCullGroup (without this, props that have a SectorCullGroup component will become invisible inappropriately)
            if (component is ISectorGroup sectorGroup)
            {
                sectorGroup.SetSector(sector);
            }

            // Not doing else if here because idk if any of the classes below implement ISectorGroup

            if (component is Sector s)
            {
                s.SetParentSector(sector);
            }

            else if (component is SectorCullGroup sectorCullGroup)
            {
                sectorCullGroup._controllingProxy = null;
                
                // fixes sector cull group deactivating renderers on map view enter and fast foward
                // TODO: does this actually work? what? how?
                sectorCullGroup._inMapView = false;
                sectorCullGroup._isFastForwarding = false;
                sectorCullGroup.SetVisible(sectorCullGroup.ShouldBeVisible(), true, false);
            }

            else if(component is SectoredMonoBehaviour behaviour)
            {
                // not using SetSector here because it registers the events twice
                // perhaps this happens with ISectorGroup.SetSector or Sector.SetParentSector too? idk and nothing seems to break because of it yet
                behaviour._sector = sector;
            }

            else if(component is OWItemSocket socket)
            {
                socket._sector = sector;
            }

            // TODO: Fix low res reels
            else if(component is SlideReelItem)
            {

            }

            else if(component is NomaiRemoteCameraPlatform remoteCameraPlatform)
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
            if (component is FogLight or SectoredMonoBehaviour or ISectorGroup)
            {
                UnityEngine.Object.DestroyImmediate(component);
                return true;
            }
            return false;
        }

        private static void FixComponent(Component component, GameObject planetGO, bool ignoreSun)
        {
            // Fix other components
            if (component is Transform)
            {
                if (!ignoreSun && component.gameObject.layer == Layer.IgnoreSun)
                {
                    component.gameObject.layer = Layer.Default;
                }
                else if (ignoreSun && component.gameObject.layer == Layer.Default)
                {
                    component.gameObject.layer = Layer.IgnoreSun;
                }
            }
            // I forget why this is here
            else if (component is GhostIK or GhostEffects)
            {
                UnityEngine.Object.DestroyImmediate(component);
                return;
            }
            else if (component is DarkMatterVolume)
            {
                var probeVisuals = component.gameObject.transform.Find("ProbeVisuals");
                if (probeVisuals != null) probeVisuals.gameObject.SetActive(true);
            }
            else if (component is DarkMatterSubmergeController submergeController)
            {
                var water = planetGO.GetComponentsInChildren<RadialFluidVolume>().FirstOrDefault(x => x._fluidType == FluidVolume.Type.WATER);
                if (submergeController._fluidDetector)
                    submergeController._fluidDetector._onlyDetectableFluid = water;
            }
            // Fix anglerfish speed on orbiting planets
            else if (component is AnglerfishController angler)
            {
                try
                {
                    angler._chaseSpeed += OWPhysics.CalculateOrbitVelocity(planetGO.GetAttachedOWRigidbody(), planetGO.GetComponent<AstroObject>().GetPrimaryBody().GetAttachedOWRigidbody()).magnitude;
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Couldn't update AnglerFish chase speed:\n{e}");
                }
            }

            // fix campfires
            else if (component is InteractVolume)
            {
                component.gameObject.AddComponent<InteractVolumeFixer>();
            }
            else if (component is PlayerAttachPoint)
            {
                component.gameObject.AddComponent<PlayerAttachPointFixer>();
            }

            else if (component is NomaiInterfaceOrb orb)
            {
                // detect planet gravity
                var gravityVolume = planetGO.GetAttachedOWRigidbody().GetAttachedGravityVolume();
                orb.GetComponent<ConstantForceDetector>()._detectableFields = gravityVolume ? new ForceVolume[] { gravityVolume } : new ForceVolume[] { };
            }

            else if (component is VisionTorchItem torchItem)
            {
                torchItem.enabled = true;
                torchItem.mindProjectorTrigger.enabled = true;
                torchItem.gameObject.AddComponent<VisionTorchItemFixer>();
            }

            else if (component is Animator animator) animator.enabled = true;
            else if(component is Collider collider) collider.enabled = true;
            // Bug 533 - Don't show the electricity effect renderers
            else if (component is Renderer renderer && component.gameObject.GetComponent<ElectricityEffect>() == null) renderer.enabled = true;
            else if(component is Shape shape) shape.enabled = true;

            // If it's not a moving anglerfish make sure the anim controller is regular
            else if(component is AnglerfishAnimController && component.transform.parent.GetComponent<AnglerfishController>() == null) //Manual parent chain so we can find inactive
            {
                component.gameObject.AddComponent<AnglerAnimFixer>();
            }
        }

        /// <summary>
        /// Has to happen after AnglerfishAnimController awake to remove the events it has set up.
        /// Otherwise results in the anglerfish 1) having its animations controlled by an actual fish 2) randomly having different animations on solarsystem load
        /// Can't do delay because it needs to work with scatter (copies a prefab made using MakeDetail).
        /// </summary>
        [RequireComponent(typeof(AnglerfishAnimController))]
        private class AnglerAnimFixer : MonoBehaviour
        {
            public void Start()
            {
                var angler = GetComponent<AnglerfishAnimController>();

                NHLogger.LogVerbose("Fixing anglerfish animation");

                // Remove any event reference to its angler
                if (angler._anglerfishController)
                {
                    angler._anglerfishController.OnChangeAnglerState -= angler.OnChangeAnglerState;
                    angler._anglerfishController.OnAnglerTurn -= angler.OnAnglerTurn;
                    angler._anglerfishController.OnAnglerSuspended -= angler.OnAnglerSuspended;
                    angler._anglerfishController.OnAnglerUnsuspended -= angler.OnAnglerUnsuspended;
                }
                angler.enabled = true;
                angler.OnChangeAnglerState(AnglerfishController.AnglerState.Lurking);
                
                Destroy(this);
            }
        }

        /// <summary>
        /// Has to happen after 1 frame to work with VR
        /// </summary>
        [RequireComponent(typeof(InteractVolume))]
        private class InteractVolumeFixer : MonoBehaviour
        {
            public void Start()
            {
                var interactVolume = GetComponent<InteractVolume>();
                interactVolume._playerCam = Locator.GetPlayerCamera();

                Destroy(this);
            }
        }

        /// <summary>
        /// Has to happen after 1 frame to work with VR
        /// </summary>
        [RequireComponent(typeof(PlayerAttachPoint))]
        private class PlayerAttachPointFixer : MonoBehaviour
        {
            public void Start()
            {
                var playerAttachPoint = GetComponent<PlayerAttachPoint>();
                playerAttachPoint._playerController = Locator.GetPlayerController();
                playerAttachPoint._playerOWRigidbody = Locator.GetPlayerBody();
                playerAttachPoint._playerTransform = Locator.GetPlayerTransform();
                playerAttachPoint._fpsCamController = Locator.GetPlayerCameraController();

                Destroy(this);
            }
        }

        /// <summary>
        /// Has to happen after 1 frame to work with VR
        /// </summary>
        [RequireComponent(typeof(VisionTorchItem))]
        private class VisionTorchItemFixer : MonoBehaviour
        {
            public void Start()
            {
                var torchItem = GetComponent<VisionTorchItem>();
                torchItem.mindSlideProjector._mindProjectorImageEffect = Locator.GetPlayerCamera().GetComponent<MindProjectorImageEffect>();

                Destroy(this);
            }
        }
    }
}
