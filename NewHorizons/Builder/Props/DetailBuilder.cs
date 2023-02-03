using NewHorizons.Builder.General;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class DetailBuilder
    {
        private static readonly Dictionary<PropModule.DetailInfo, GameObject> _detailInfoToCorrespondingSpawnedGameObject = new();
        private static readonly Dictionary<(Sector, string), (GameObject prefab, bool isItem)> _fixedPrefabCache = new();

        static DetailBuilder()
        {
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private static void SceneManager_sceneUnloaded(Scene scene)
        {
            _fixedPrefabCache.Clear();
            _detailInfoToCorrespondingSpawnedGameObject.Clear();
        }

        public static GameObject GetSpawnedGameObjectByDetailInfo(PropModule.DetailInfo detail)
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

            GameObject prop;
            bool isItem;

            // We save copies with all their components fixed, good if the user is placing the same detail more than once
            if (detail?.path != null && _fixedPrefabCache.TryGetValue((sector, detail.path), out var storedPrefab))
            {
                prop = storedPrefab.prefab.InstantiateInactive();
                prop.name = prefab.name;
                isItem = storedPrefab.isItem;
            }
            else
            {
                prop = prefab.InstantiateInactive();
                prop.name = prefab.name;

                StreamingHandler.SetUpStreaming(prop, sector);

                // Could check this in the for loop but I'm not sure what order we need to know about this in
                var isTorch = prop.GetComponent<VisionTorchItem>() != null;
                isItem = false;

                foreach (var component in prop.GetComponentsInChildren<Component>(true))
                {
                    if (component.gameObject == prop && component is OWItem) isItem = true;

                    if (sector == null)
                    {
                        if (FixUnsectoredComponent(component)) continue;
                    }
                    else FixSectoredComponent(component, sector, isTorch);

                    FixComponent(component, go);
                }

                if (detail.path != null)
                {
                    _fixedPrefabCache.Add((sector, detail.path), (prop.InstantiateInactive(), isItem));
                }
            }

            prop.transform.parent = sector?.transform ?? go.transform;

            // Items shouldn't use these else they get weird
            if (isItem) detail.keepLoaded = true;

            prop.transform.position = detail.position == null ? go.transform.position : go.transform.TransformPoint(detail.position);

            Quaternion rot = detail.rotation == null ? Quaternion.identity : Quaternion.Euler(detail.rotation);

            if (detail.alignToNormal)
            {
                // Apply the rotation after aligning it with normal
                var up = (prop.transform.position - go.transform.position).normalized;
                prop.transform.rotation = Quaternion.FromToRotation(Vector3.up, up);
                prop.transform.rotation *= rot;
            }
            else
            {
                prop.transform.rotation = go.transform.TransformRotation(rot);
            }

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

            if (detail.isRelativeToParent)
            {
                prop.transform.localPosition = detail.position == null ? Vector3.zero : detail.position;
                if (detail.alignToNormal)
                {
                    // Apply the rotation after aligning it with normal
                    var up = (prop.transform.position - go.transform.position).normalized;
                    prop.transform.rotation = Quaternion.FromToRotation(Vector3.up, up);
                    prop.transform.rotation *= rot;
                }
                else
                {
                    prop.transform.localRotation = rot;
                }
            }
            
            if (isItem)
            {
                // Else when you put them down you can't pick them back up
                var col = prop.GetComponent<OWCollider>();
                if (col != null) col._physicsRemoved = false;
            }

            if (!detail.keepLoaded) GroupsBuilder.Make(prop, sector);
            prop.SetActive(true);

            if (detail.hasPhysics || true)
            {
                var addPhysics = prop.AddComponent<AddPhysics>();
                addPhysics.Sector = sector;
                addPhysics.Radius = detail.physicsRadius;
            }

            _detailInfoToCorrespondingSpawnedGameObject[detail] = prop;

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

        private static void FixComponent(Component component, GameObject planetGO)
        {
            // Fix other components
            // I forget why this is here
            if (component is GhostIK or GhostEffects)
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

            if (component is Animator animator) animator.enabled = true;
            if (component is Collider collider) collider.enabled = true;
            if (component is Renderer renderer) renderer.enabled = true;
            if (component is Shape shape) shape.enabled = true;
            
            // fixes sector cull group deactivating renderers on map view enter and fast foward
            // TODO: does this actually work? what? how?
            if (component is SectorCullGroup sectorCullGroup)
            {
                sectorCullGroup._inMapView = false;
                sectorCullGroup._isFastForwarding = false;
                sectorCullGroup.SetVisible(sectorCullGroup.ShouldBeVisible(), true, false);
            }
            
            // If it's not a moving anglerfish make sure the anim controller is regular
            if (component is AnglerfishAnimController && component.GetComponentInParent<AnglerfishController>() == null)
                component.gameObject.AddComponent<AnglerAnimFixer>();
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
                
                Logger.LogVerbose("Fixing anglerfish animation");

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

        // TODO: simulate in sector
        // BUG: detector collider is not included in groups
        // TODO: mass
        private class AddPhysics : MonoBehaviour
        {
            public Sector Sector;
            public float? Radius;

            private IEnumerator Start()
            {
                if (!Sector)
                    Logger.LogError($"Prop {name} has physics but no sector! This will fall through things when surrounding area is unloaded");

                yield return new WaitForSeconds(.1f);

                var parentBody = GetComponentInParent<OWRigidbody>();

                // just disable all non triggers
                foreach (var meshCollider in GetComponentsInChildren<MeshCollider>(true))
                    if (!meshCollider.isTrigger)
                        meshCollider.enabled = false;

                var bodyGo = new GameObject($"{name}_Body");
                bodyGo.SetActive(false);
                bodyGo.transform.position = gameObject.transform.position;
                bodyGo.transform.rotation = gameObject.transform.rotation;

                var owRigidbody = bodyGo.AddComponent<OWRigidbody>();
                owRigidbody._simulateInSector = Sector;

                var detector = new GameObject("Detector");
                detector.transform.SetParent(bodyGo.transform, false);
                detector.layer = LayerMask.NameToLayer("AdvancedDetector");
                detector.tag = "DynamicPropDetector";
                var sphereCollider = detector.AddComponent<SphereCollider>();
                if (Radius.HasValue)
                    sphereCollider.radius = Radius.Value;
                detector.AddComponent<DynamicForceDetector>();
                detector.AddComponent<DynamicFluidDetector>();

                var impactSensor = bodyGo.AddComponent<ImpactSensor>();
                var impactAudio = new GameObject("ImpactAudio");
                impactAudio.transform.SetParent(bodyGo.transform, false);
                var audioSource = impactAudio.AddComponent<AudioSource>();
                audioSource.maxDistance = 30;
                audioSource.dopplerLevel = 0;
                audioSource.rolloffMode = AudioRolloffMode.Custom;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1;
                var owAudioSource = impactAudio.AddComponent<OWAudioSource>();
                owAudioSource._audioSource = audioSource;
                owAudioSource._track = OWAudioMixer.TrackName.Environment;
                var objectImpactAudio = impactAudio.AddComponent<ObjectImpactAudio>();
                objectImpactAudio._minPitch = 0.4f;
                objectImpactAudio._maxPitch = 0.6f;
                objectImpactAudio._impactSensor = impactSensor;

                bodyGo.SetActive(true);

                owRigidbody.SetVelocity(parentBody.GetPointVelocity(transform.position));
                transform.SetParent(bodyGo.transform, false);

                Destroy(this);
            }
        }

        /*
        private static void AddPhysics(GameObject prop, Sector sector)
        {
            var primaryBody = prop.GetComponentInParent<OWRigidbody>();

            var rb = prop.AddComponent<Rigidbody>();
            var owrb = prop.AddComponent<OWRigidbody>();
            var kine = prop.AddComponent<KinematicRigidbody>();
            rb.isKinematic = true;
            var offsetApplier = prop.AddComponent<CenterOfTheUniverseOffsetApplier>();
            offsetApplier.Init(owrb);
            owrb._simulateInSector = sector;
            owrb._kinematicSimulation = true;
            owrb._kinematicRigidbody = kine;
            owrb._offsetApplier = offsetApplier;

            prop.AddComponent<InitialMotion>().SetPrimaryBody(primaryBody);
            prop.AddComponent<MatchInitialMotion>().SetBodyToMatch(primaryBody);

            var detector = new GameObject("Detector");
            detector.transform.parent = prop.transform;
            detector.transform.localPosition = Vector3.zero;
            detector.tag = "DynamicPropDetector";

            var shape = detector.AddComponent<SphereShape>();
            shape.SetCollisionMode(Shape.CollisionMode.Detector);
            shape.SetLayer(Shape.Layer.Default);
            shape.layerMask = 5;
            detector.AddComponent<DynamicForceDetector>();
            detector.AddComponent<ForceApplier>();
        }
        */
    }
}