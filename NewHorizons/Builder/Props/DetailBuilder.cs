using NewHorizons.Builder.General;
using NewHorizons.Components;
using NewHorizons.Components.Props;
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
        private static GameObject _emptyPrefab;

        static DetailBuilder()
        {
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        #region obsolete
        // Never change method signatures, people directly reference the NH dll and it can break backwards compatibility
        // In particular, Outer Wives needs this method signature
        [Obsolete]
        public static GameObject Make(GameObject go, Sector sector, GameObject prefab, DetailInfo detail)
            => Make(go, sector, mod: null, prefab, detail);

        // Dreamstalker needed this one
        [Obsolete]
        public static GameObject Make(GameObject go, Sector sector, DetailInfo detail)
            => Make(go, sector, mod: null, detail);
        #endregion

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
        public static GameObject Make(GameObject planetGO, Sector sector, IModBehaviour mod, DetailInfo info)
        {
            if (sector == null) info.keepLoaded = true;

            if (info.assetBundle != null)
            {
                // Shouldn't happen
                if (mod == null) return null;

                return Make(planetGO, sector, mod, AssetBundleUtilities.LoadPrefab(info.assetBundle, info.path, mod), info);
            }

            if (_emptyPrefab == null) _emptyPrefab = new GameObject("Empty");

            // Allow for empty game objects so you can set up conditional activation on them and parent other props to them
            var prefab = string.IsNullOrEmpty(info.path) ? _emptyPrefab : SearchUtilities.Find(info.path);

            if (prefab == null)
            {
                NHLogger.LogError($"Couldn't find detail {info.path}");
                return null;
            }
            else
            {
                return Make(planetGO, sector, mod, prefab, info);
            }
        }

        /// <summary>
        /// Create a detail using a prefab.
        /// </summary>
        public static GameObject Make(GameObject go, Sector sector, IModBehaviour mod, GameObject prefab, DetailInfo detail)
        {
            if (prefab == null) return null;

            if (sector == null) detail.keepLoaded = true;

            GameObject prop;
            bool isItem;
            bool invalidComponentFound = false;
            bool isFromAssetBundle = !string.IsNullOrEmpty(detail.assetBundle);

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
                isItem = false;

                var existingSectors = new HashSet<Sector>(prop.GetComponentsInChildren<Sector>(true));

                foreach (var component in prop.GetComponentsInChildren<Component>(true))
                {
                    // Rather than having the entire prop not exist when a detail fails let's just try-catch and log an error
                    try
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
                        else
                        {
                            // Fix cull groups only when not from an asset bundle (because then they're there on purpose!)
                            // keepLoaded should remove existing groups
                            // renderers/colliders get enabled later so we dont have to do that here
                            if (detail.keepLoaded && !isFromAssetBundle && component is SectorCullGroup or SectorCollisionGroup or SectorLightsCullGroup)
                            {
                                UnityEngine.Object.DestroyImmediate(component);
                                continue;
                            }

                            FixSectoredComponent(component, sector, existingSectors);
                        }

                        // Asset bundle is a real string -> Object loaded from unity
                        // If they're adding dialogue we have to manually register the xml text
                        if (isFromAssetBundle && component is CharacterDialogueTree dialogue)
                        {
                            DialogueBuilder.HandleUnityCreatedDialogue(dialogue);
                        }

                        // copied details need their lanterns fixed
                        if (!isFromAssetBundle && component is DreamLanternController lantern)
                        {
                            lantern.gameObject.AddComponent<DreamLanternControllerFixer>();
                        }

                        FixComponent(component, go, detail.ignoreSun);
                    }
                    catch(Exception e)
                    {
                        NHLogger.LogError($"Failed to correct component {component?.GetType()?.Name} on {go?.name} - {e}");
                    }
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

            if (detail.item != null)
            {
                ItemBuilder.MakeItem(prop, go, sector, detail.item, mod);
                isItem = true;
                if (detail.hasPhysics)
                {
                    NHLogger.LogWarning($"An item with the path {detail.path} has both '{nameof(DetailInfo.hasPhysics)}' and '{nameof(DetailInfo.item)}' set. This will usually result in undesirable behavior.");
                }
            }

            if (detail.itemSocket != null)
            {
                ItemBuilder.MakeSocket(prop, go, sector, detail.itemSocket);
            }

            // Items should always be kept loaded else they will vanish in your hand as you leave the sector
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
                newDetailGO.transform.parent = prop.transform.parent;
                newDetailGO.transform.position = prop.transform.position;
                newDetailGO.transform.rotation = prop.transform.rotation;
                newDetailGO.transform.localScale = prop.transform.localScale;

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
                addPhysics.Sector = detail.keepLoaded ? null : sector;
                addPhysics.Mass = detail.physicsMass;
                addPhysics.Radius = detail.physicsRadius;
                addPhysics.SuspendUntilImpact = detail.physicsSuspendUntilImpact;
            }

            if (!string.IsNullOrEmpty(detail.activationCondition))
            {
                ConditionalObjectActivation.SetUp(prop, detail.activationCondition, detail.blinkWhenActiveChanged, true);   
            }
            if (!string.IsNullOrEmpty(detail.deactivationCondition))
            {
                ConditionalObjectActivation.SetUp(prop, detail.deactivationCondition, detail.blinkWhenActiveChanged, false);
            }

            _detailInfoToCorrespondingSpawnedGameObject[detail] = prop;

            return prop;
        }

        /// <summary>
        /// Fix components that have sectors. Has a specific fix if there is a VisionTorchItem on the object.
        /// </summary>
        private static void FixSectoredComponent(Component component, Sector sector, HashSet<Sector> existingSectors)
        {
            // fix Sector stuff, eg SectorCullGroup (without this, props that have a SectorCullGroup component will become invisible inappropriately)
            if (component is ISectorGroup sectorGroup && !existingSectors.Contains(sectorGroup.GetSector()))
            {
                sectorGroup.SetSector(sector);
            }

            // Not doing else if here because idk if any of the classes below implement ISectorGroup

            if(component is SectoredMonoBehaviour behaviour && !existingSectors.Contains(behaviour._sector))
            {
                // not using SetSector here because it registers the events twice
                // perhaps this happens with ISectorGroup.SetSector or Sector.SetParentSector too? idk and nothing seems to break because of it yet
                behaviour._sector = sector;
            }

            else if(component is OWItemSocket socket && !existingSectors.Contains(socket._sector))
            {
                socket._sector = sector;
            }

            // TODO: Fix low res reels (probably in VanillaFix since its a vanilla bug)
            else if(component is SlideReelItem)
            {

            }

            else if(component is NomaiRemoteCameraPlatform remoteCameraPlatform && !existingSectors.Contains(remoteCameraPlatform._visualSector))
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
                // somehow Intervention has GetAttachedOWRigidbody as null sometimes, idk why
                var gravityVolume = planetGO.GetAttachedOWRigidbody()?.GetAttachedGravityVolume();
                orb.GetComponent<ConstantForceDetector>()._detectableFields = gravityVolume ? new ForceVolume[] { gravityVolume } : new ForceVolume[0];
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

            // If it's not a moving ghostbird (ie Prefab_IP_GhostBird/Ghostbird_IP_ANIM) make sure it doesnt spam NREs
            // Manual parent chain so we can find inactive
            else if (component is GhostIK or GhostEffects && component.transform.parent.GetComponent<GhostBrain>() == null)
            {
                UnityEngine.Object.DestroyImmediate(component);
            }
            // If it's not a moving anglerfish (ie Anglerfish_Body/Beast_Anglerfish) make sure the anim controller is regular
            // Manual parent chain so we can find inactive
            else if(component is AnglerfishAnimController && component.transform.parent.GetComponent<AnglerfishController>() == null)
            {
                component.gameObject.AddComponent<AnglerAnimFixer>();
            }
            // Add custom logic to NH-spawned rafts to handle fluid changes
            else if (component is RaftController raft)
            {
                component.gameObject.AddComponent<NHRaftController>();
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

                // Remove any event reference to its angler so that they dont change its state
                if (angler._anglerfishController)
                {
                    angler._anglerfishController.OnChangeAnglerState -= angler.OnChangeAnglerState;
                    angler._anglerfishController.OnAnglerTurn -= angler.OnAnglerTurn;
                    angler._anglerfishController.OnAnglerSuspended -= angler.OnAnglerSuspended;
                    angler._anglerfishController.OnAnglerUnsuspended -= angler.OnAnglerUnsuspended;
                }
                // Disable the angler anim controller because we don't want Update or LateUpdate to run, just need it to set the initial Animator state
                angler.enabled = false;
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

        /// <summary>
        /// need component here to run after DreamLanternController.Awake
        /// </summary>
        [RequireComponent(typeof(DreamLanternController))]
        private class DreamLanternControllerFixer : MonoBehaviour
        {
            private void Start()
            {
                // based on https://github.com/Bwc9876/OW-Amogus/blob/master/Amogus/LanternCreator.cs
                // needed to fix petals looking backwards, among other things

                var lantern = GetComponent<DreamLanternController>();

                // this is set in Awake, we wanna override it

                // Manually copied these values from a artifact lantern so that we don't have to find it (works in Eye)
                lantern._origLensFlareBrightness = 0f;
                lantern._focuserPetalsBaseEulerAngles = new Vector3[] 
                { 
                    new Vector3(0.7f, 270.0f, 357.5f), 
                    new Vector3(288.7f, 270.1f, 357.4f), 
                    new Vector3(323.3f, 90.0f, 177.5f),
                    new Vector3(35.3f, 90.0f, 177.5f), 
                    new Vector3(72.7f, 270.1f, 357.5f) 
                };
                lantern._dirtyFlag_focus = true;
                lantern._concealerRootsBaseScale = new Vector3[] 
                {
                    Vector3.one,
                    Vector3.one,
                    Vector3.one
                };
                lantern._concealerCoversStartPos = new Vector3[] 
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(0.0f, -0.1f, 0.0f),
                    new Vector3(0.0f, -0.2f, 0.0f),
                    new Vector3(0.0f, 0.2f, 0.0f),
                    new Vector3(0.0f, 0.1f, 0.0f),
                    new Vector3(0.0f, 0.0f, 0.0f)
                };
                lantern._dirtyFlag_concealment = true;
                lantern.UpdateVisuals();
                
                Destroy(this);
            }
        }
    }
}
