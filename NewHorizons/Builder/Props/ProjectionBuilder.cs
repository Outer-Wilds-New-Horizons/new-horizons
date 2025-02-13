using HarmonyLib;
using NewHorizons.Components.EOTE;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.EchoesOfTheEye;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static NewHorizons.Utility.Files.AssetBundleUtilities;

namespace NewHorizons.Builder.Props
{
    public static class ProjectionBuilder
    {
        public static string CurrentSlideReelFolder => "SlideReelCache_" + Main.Instance.CurrentStarSystem;
        public static string InvertedSlideReelCacheFolder => CurrentSlideReelFolder  + "/Inverted";
        public static string AtlasSlideReelCacheFolder => CurrentSlideReelFolder + "/Atlas";

        public static GameObject SlideReelWholePrefab { get; private set; }
        public static GameObject SlideReelWholePristinePrefab { get; private set; }
        public static GameObject SlideReelWholeRustedPrefab { get; private set; }
        public static GameObject SlideReelWholeDestroyedPrefab { get; private set; }
        public static GameObject SlideReel8Prefab { get; private set; }
        public static GameObject SlideReel8PristinePrefab { get; private set; }
        public static GameObject SlideReel8RustedPrefab { get; private set; }
        public static GameObject SlideReel8DestroyedPrefab { get; private set; }
        public static GameObject SlideReel7Prefab { get; private set; }
        public static GameObject SlideReel7PristinePrefab { get; private set; }
        public static GameObject SlideReel7RustedPrefab { get; private set; }
        public static GameObject SlideReel7DestroyedPrefab { get; private set; }
        public static GameObject SlideReel6Prefab { get; private set; }
        public static GameObject SlideReel6PristinePrefab { get; private set; }
        public static GameObject SlideReel6RustedPrefab { get; private set; }
        public static GameObject SlideReel6DestroyedPrefab { get; private set; }

        private static GameObject _autoPrefab;
        private static GameObject _visionTorchDetectorPrefab;
        private static GameObject _standingVisionTorchPrefab;
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");

        private static bool _isInit;

        public static bool CacheExists(IModBehaviour mod) => Directory.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, AtlasSlideReelCacheFolder));

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            SlideReelWholePrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_Whole");
            SlideReelWholePristinePrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_DW_Reel_Whole");
            SlideReelWholeRustedPrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_Rusted_Whole");
            SlideReelWholeDestroyedPrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_Destroyed_Whole");
            SlideReel8Prefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_8");
            SlideReel8PristinePrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_DW_Reel_8");
            SlideReel8RustedPrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_Rusted_8");
            SlideReel8DestroyedPrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_Destroyed_8");
            SlideReel7Prefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_7");
            SlideReel7PristinePrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_DW_Reel_7");
            SlideReel7RustedPrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_Rusted_7");
            SlideReel7DestroyedPrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_Destroyed_7");
            SlideReel6Prefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_6");
            SlideReel6PristinePrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_DW_Reel_6");
            SlideReel6RustedPrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_Rusted_6");
            SlideReel6DestroyedPrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_IP_Reel_Destroyed_6");

            if (_autoPrefab == null)
            {
                _autoPrefab = SearchUtilities.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone4/Sector_BlightedShore/Sector_JammingControlRoom_Zone4/Interactables_JammingControlRoom_Zone4/AutoProjector_SignalJammer/Prefab_IP_AutoProjector_SignalJammer")?.gameObject?.InstantiateInactive()?.Rename("Prefab_IP_AutoProjector")?.DontDestroyOnLoad();
                if (_autoPrefab == null)
                    NHLogger.LogWarning($"Tried to make auto projector prefab but couldn't. Do you have the DLC installed?");
                else
                    _autoPrefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
            }

            if (_visionTorchDetectorPrefab == null)
            {
                _visionTorchDetectorPrefab = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_Underground/Sector_PrisonCell/Ghosts_PrisonCell/GhostDirector_Prisoner/Prefab_IP_GhostBird_Prisoner/Ghostbird_IP_ANIM/Ghostbird_Skin_01:Ghostbird_Rig_V01:Base/Ghostbird_Skin_01:Ghostbird_Rig_V01:Root/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine01/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine02/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine03/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine04/Ghostbird_Skin_01:Ghostbird_Rig_V01:Neck01/Ghostbird_Skin_01:Ghostbird_Rig_V01:Neck02/Ghostbird_Skin_01:Ghostbird_Rig_V01:Head/PrisonerHeadDetector")?.gameObject?.InstantiateInactive()?.Rename("Prefab_IP_VisionTorchDetector")?.DontDestroyOnLoad();
                if (_visionTorchDetectorPrefab == null)
                    NHLogger.LogWarning($"Tried to make vision torch detector prefab but couldn't. Do you have the DLC installed?");
                else
                    _visionTorchDetectorPrefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
            }

            if (_standingVisionTorchPrefab == null)
            {
                _standingVisionTorchPrefab = SearchUtilities.Find("RingWorld_Body/Sector_RingWorld/Sector_SecretEntrance/Interactibles_SecretEntrance/Experiment_1/VisionTorchApparatus/VisionTorchRoot/Prefab_IP_VisionTorchProjector")?.gameObject?.InstantiateInactive()?.Rename("Prefab_IP_VisionTorchProjector")?.DontDestroyOnLoad();
                if (_standingVisionTorchPrefab == null)
                    NHLogger.LogWarning($"Tried to make standing vision torch prefab but couldn't. Do you have the DLC installed?");
                else
                    _standingVisionTorchPrefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
            }
        }

        public static void Make(GameObject go, Sector sector, ProjectionInfo info, IModBehaviour mod)
        {
            switch (info.type)
            {
                case ProjectionInfo.SlideShowType.AutoProjector:
                    MakeAutoProjector(go, sector, info, mod);
                    break;
                case ProjectionInfo.SlideShowType.SlideReel:
                    MakeSlideReel(go, sector, info, mod);
                    break;
                case ProjectionInfo.SlideShowType.VisionTorchTarget:
                    MakeMindSlidesTarget(go, sector, info, mod);
                    break;
                case ProjectionInfo.SlideShowType.StandingVisionTorch:
                    MakeStandingVisionTorch(go, sector, info, mod);
                    break;
                default:
                    NHLogger.LogError($"Invalid projection type {info.type}");
                    break;
            }
        }

        public static string GetUniqueSlideReelID(IModBehaviour mod, SlideInfo[] slides) => $"{mod.ModHelper.Manifest.UniqueName}{slides.Join(x => x.imagePath)}".GetHashCode().ToString();

        private static GameObject MakeSlideReel(GameObject planetGO, Sector sector, ProjectionInfo info, IModBehaviour mod)
        {
            InitPrefabs();

            GameObject prefab = GetSlideReelPrefab(info.reelModel, info.reelCondition);

            if (prefab == null) return null;

            var slideReelObj = GeneralPropBuilder.MakeFromPrefab(prefab, $"Prefab_IP_Reel_{GetSlideReelName(info.reelModel, info.reelCondition)}_{mod.ModHelper.Manifest.Name}", planetGO, sector, info);

            var slideReel = slideReelObj.GetComponent<SlideReelItem>();
            slideReel.SetSector(sector);
            slideReel.SetVisible(true);

            var toDestroy = slideReelObj.GetComponent<SlideCollectionContainer>();
            var slideCollectionContainer = slideReelObj.AddComponent<NHSlideCollectionContainer>();
            slideReel._slideCollectionContainer = slideCollectionContainer;
            Component.DestroyImmediate(toDestroy);

            foreach (var renderer in slideReelObj.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }

            // Now we replace the slides
            int slidesCount = info.slides.Length;
            SlideCollection slideCollection = new NHSlideCollection(slidesCount, mod, info.slides.Select(x => x.imagePath).ToArray());
            slideCollection.streamingAssetIdentifier = string.Empty; // NREs if null

            // We can fit 16 slides max into an atlas
            var textures = new Texture2D[slidesCount > 16 ? 16 : slidesCount];

            // Slide reels dynamically load the inverted cached images when needed. We only need to load raw images to generate the cache or atlases
            var (_, atlasImageLoader, imageLoader) = StartAsyncLoader(mod, info.slides, ref slideCollection, false, true, false);

            // If the cache doesn't exist it will be created here, slide reels only use the base image loader for cache creation so delete the images after to free memory
            imageLoader.deleteTexturesWhenDone = true;

            var key = GetUniqueSlideReelID(mod, info.slides);

            if (atlasImageLoader != null)
            {
                atlasImageLoader.imageLoadedEvent.AddListener(
                    (Texture2D tex, int _, string originalPath) =>
                    {
                        // all textures required to build the reel's textures have been loaded
                        var slidesBack = slideReelObj.GetComponentInChildren<TransformAnimator>(true).transform.Find("Slides_Back").GetComponent<MeshRenderer>();
                        var slidesFront = slideReelObj.GetComponentInChildren<TransformAnimator>(true).transform.Find("Slides_Front").GetComponent<MeshRenderer>();

                        // Now put together the textures into a 4x4 thing for the materials
                        var reelTexture = tex;
                        slidesBack.material.mainTexture = reelTexture;
                        slidesBack.material.SetTexture(EmissionMap, reelTexture);
                        slidesBack.material.name = reelTexture.name;
                        slidesFront.material.mainTexture = reelTexture;
                        slidesFront.material.SetTexture(EmissionMap, reelTexture);
                        slidesFront.material.name = reelTexture.name;
                    }
                );
            }
            else
            {
                // this variable just lets us track how many of the first 15 slides have been loaded.
                // this way as soon as the last one is loaded (due to async loading, this may be
                // slide 7, or slide 3, or whatever), we can build the slide reel texture. This allows us
                // to avoid doing a "is every element in the array `textures` not null" check every time a texture finishes loading
                int displaySlidesLoaded = 0;
                imageLoader.imageLoadedEvent.AddListener(
                (Texture2D tex, int index, string originalPath) =>
                {
                    var time = DateTime.Now;

                    slideCollection.slides[index]._image = ImageUtilities.InvertSlideReel(mod, tex, originalPath);
                    NHLogger.LogVerbose($"Slide reel make reel invert texture {(DateTime.Now - time).TotalMilliseconds}ms");
                    // Track the first 16 to put on the slide reel object
                    if (index < textures.Length)
                    {
                        textures[index] = tex;
                        displaySlidesLoaded++;
                        if (displaySlidesLoaded == textures.Length)
                        {
                            // all textures required to build the reel's textures have been loaded
                            var slidesBack = slideReelObj.GetComponentInChildren<TransformAnimator>(true).transform.Find("Slides_Back").GetComponent<MeshRenderer>();
                            var slidesFront = slideReelObj.GetComponentInChildren<TransformAnimator>(true).transform.Find("Slides_Front").GetComponent<MeshRenderer>();

                            // Now put together the textures into a 4x4 thing for the materials #888
                            var displayTextures = textures;
                            if (info.displaySlides != null && info.displaySlides.Length > 0)
                            {
                                displayTextures = info.displaySlides.Select(x => textures[x]).ToArray();
                            }

                            var reelTexture = ImageUtilities.MakeReelTexture(mod, displayTextures, key);
                            slidesBack.material.mainTexture = reelTexture;
                            slidesBack.material.SetTexture(EmissionMap, reelTexture);
                            slidesBack.material.name = reelTexture.name;
                            slidesFront.material.mainTexture = reelTexture;
                            slidesFront.material.SetTexture(EmissionMap, reelTexture);
                            slidesFront.material.name = reelTexture.name;
                        }
                    }

                    NHLogger.LogVerbose($"Slide reel make reel texture {(DateTime.Now - time).TotalMilliseconds}ms");
                });
            }

            // Else when you put them down you can't pick them back up
            slideReelObj.GetComponent<OWCollider>()._physicsRemoved = false;

            slideCollectionContainer.slideCollection = slideCollection;

            LinkShipLogFacts(info, slideCollectionContainer);

            StreamingHandler.SetUpStreaming(slideReelObj, sector);

            slideReelObj.SetActive(true);

            return slideReelObj;
        }

        private static GameObject GetSlideReelPrefab(ProjectionInfo.SlideReelType model, ProjectionInfo.SlideReelCondition condition)
        {
            switch (model)
            {
                case ProjectionInfo.SlideReelType.SixSlides:
                    {
                        switch (condition)
                        {
                            case ProjectionInfo.SlideReelCondition.Antique:
                            default:
                                return SlideReel6Prefab;
                            case ProjectionInfo.SlideReelCondition.Pristine:
                                return SlideReel6PristinePrefab;
                            case ProjectionInfo.SlideReelCondition.Rusted:
                                return SlideReel6RustedPrefab;
                        }
                    }
                case ProjectionInfo.SlideReelType.SevenSlides:
                default:
                    {
                        switch (condition)
                        {
                            case ProjectionInfo.SlideReelCondition.Antique:
                            default:
                                return SlideReel7Prefab;
                            case ProjectionInfo.SlideReelCondition.Pristine:
                                return SlideReel7PristinePrefab;
                            case ProjectionInfo.SlideReelCondition.Rusted:
                                return SlideReel7RustedPrefab;
                        }
                    }
                case ProjectionInfo.SlideReelType.EightSlides:
                    {
                        switch (condition)
                        {
                            case ProjectionInfo.SlideReelCondition.Antique:
                            default:
                                return SlideReel8Prefab;
                            case ProjectionInfo.SlideReelCondition.Pristine:
                                return SlideReel8PristinePrefab;
                            case ProjectionInfo.SlideReelCondition.Rusted:
                                return SlideReel8RustedPrefab;
                        }
                    }
                case ProjectionInfo.SlideReelType.Whole:
                    {
                        switch (condition)
                        {
                            case ProjectionInfo.SlideReelCondition.Antique:
                            default:
                                return SlideReelWholePrefab;
                            case ProjectionInfo.SlideReelCondition.Pristine:
                                return SlideReelWholePristinePrefab;
                            case ProjectionInfo.SlideReelCondition.Rusted:
                                return SlideReelWholeRustedPrefab;
                        }
                    }
            }
        }

        public static string GetSlideReelName(ProjectionInfo.SlideReelType model, ProjectionInfo.SlideReelCondition condition)
        {
            switch (model)
            {
                case ProjectionInfo.SlideReelType.SixSlides:
                    return $"6_{condition}";
                case ProjectionInfo.SlideReelType.SevenSlides:
                    return $"7_{condition}";
                case ProjectionInfo.SlideReelType.EightSlides:
                    return $"8_{condition}";
                case ProjectionInfo.SlideReelType.Whole:
                default:
                    return $"{model}_{condition}";
            }
        }

        public static int GetSlideCount(ProjectionInfo.SlideReelType model)
        {
            switch (model)
            {
                case ProjectionInfo.SlideReelType.SixSlides:
                    return 6;
                case ProjectionInfo.SlideReelType.SevenSlides:
                case ProjectionInfo.SlideReelType.Whole:
                    return 7;
                case ProjectionInfo.SlideReelType.EightSlides:
                default:
                    return 8;
            }
        }

        public static GameObject MakeAutoProjector(GameObject planetGO, Sector sector, ProjectionInfo info, IModBehaviour mod)
        {
            InitPrefabs();

            if (_autoPrefab == null) return null;

            var projectorObj = GeneralPropBuilder.MakeFromPrefab(_autoPrefab, $"Prefab_IP_AutoProjector_{mod.ModHelper.Manifest.Name}", planetGO, sector, info);

            var autoProjector = projectorObj.GetComponent<AutoSlideProjector>();
            autoProjector._sector = sector;

            var toDestroy = autoProjector.GetComponent<SlideCollectionContainer>();
            var slideCollectionContainer = autoProjector.gameObject.AddComponent<NHSlideCollectionContainer>();
            slideCollectionContainer.doAsyncLoading = false;
            autoProjector._slideCollectionItem = slideCollectionContainer;
            Component.DestroyImmediate(toDestroy);

            // Now we replace the slides
            int slidesCount = info.slides.Length;
            SlideCollection slideCollection = new NHSlideCollection(slidesCount, mod, info.slides.Select(x => x.imagePath).ToArray());
            slideCollection.streamingAssetIdentifier = string.Empty; // NREs if null

            var (invImageLoader, _, imageLoader) = StartAsyncLoader(mod, info.slides, ref slideCollection, true, false, false);

            // Autoprojector only uses the inverted images so the original can be destroyed if they are loaded (when creating the cached inverted images)
            imageLoader.deleteTexturesWhenDone = true;

            if (invImageLoader != null)
            {
                // Loaded directly from cache
                invImageLoader.imageLoadedEvent.AddListener((Texture2D tex, int index, string originalPath) =>
                {
                    slideCollection.slides[index]._image = tex;
                });
            }
            else
            {
                // Create the inverted cache from existing images
                imageLoader.imageLoadedEvent.AddListener((Texture2D tex, int index, string originalPath) =>
                {
                    var time = DateTime.Now;
                    slideCollection.slides[index]._image = ImageUtilities.InvertSlideReel(mod, tex, originalPath);
                    NHLogger.LogVerbose($"Slide reel invert time {(DateTime.Now - time).TotalMilliseconds}ms");
                });
            }

            slideCollectionContainer.slideCollection = slideCollection;
            slideCollectionContainer._playWithShipLogFacts = Array.Empty<string>(); // else it NREs in container initialize

            StreamingHandler.SetUpStreaming(projectorObj, sector);

            // Change the picture on the lens
            var lens = projectorObj.transform.Find("Spotlight/Prop_IP_SingleSlideProjector/Projector_Lens").GetComponent<MeshRenderer>();
            lens.materials[1].mainTexture = slideCollection.slides[0]._image;
            lens.materials[1].SetTexture(EmissionMap, slideCollection.slides[0]._image);

            projectorObj.SetActive(true);

            return projectorObj;
        }

        // Makes a target for a vision torch to scan
        public static GameObject MakeMindSlidesTarget(GameObject planetGO, Sector sector, ProjectionInfo info, IModBehaviour mod)
        {
            InitPrefabs();

            if (_visionTorchDetectorPrefab == null) return null;

            // spawn a trigger for the vision torch
            var visionTorchTargetGO = DetailBuilder.Make(planetGO, sector, mod, _visionTorchDetectorPrefab, new DetailInfo(info) { scale = 2, rename = !string.IsNullOrEmpty(info.rename) ? info.rename : "VisionStaffDetector" });

            if (visionTorchTargetGO == null)
            {
                NHLogger.LogWarning($"Tried to make a vision torch target but couldn't. Do you have the DLC installed?");
                return null;
            }

            // The number of slides is unlimited, 15 is only for texturing the actual slide reel item. This is not a slide reel item
            var slides = info.slides;
            var slidesCount = slides.Length;
            var slideCollection = new SlideCollection(slidesCount); // TODO: uh I think that info.slides[i].playTimeDuration is not being read here... note to self for when I implement support for that: 0.7 is what to default to if playTimeDuration turns out to be 0
            slideCollection.streamingAssetIdentifier = string.Empty; // NREs if null

            var (_, _, imageLoader) = StartAsyncLoader(mod, info.slides, ref slideCollection, false, false, true);
            imageLoader.imageLoadedEvent.AddListener((Texture2D tex, int index, string originalPath) =>
            {
                var time = DateTime.Now;
                slideCollection.slides[index]._image = tex;
                NHLogger.LogVerbose($"Slide reel set time {(DateTime.Now - time).TotalMilliseconds}ms");
            });

            // attach a component to store all the data for the slides that play when a vision torch scans this target
            var target = visionTorchTargetGO.AddComponent<VisionTorchTarget>();
            var slideCollectionContainer = visionTorchTargetGO.AddComponent<NHSlideCollectionContainer>();
            slideCollectionContainer.doAsyncLoading = false;
            slideCollectionContainer.slideCollection = slideCollection;
            target.slideCollection = visionTorchTargetGO.AddComponent<MindSlideCollection>();
            target.slideCollection._slideCollectionContainer = slideCollectionContainer;

            LinkShipLogFacts(info, slideCollectionContainer);

            visionTorchTargetGO.SetActive(true);

            return visionTorchTargetGO;
        }

        public static GameObject MakeStandingVisionTorch(GameObject planetGO, Sector sector, ProjectionInfo info, IModBehaviour mod)
        {
            InitPrefabs();

            if (_standingVisionTorchPrefab == null) return null;

            // Spawn the torch itself
            var standingTorch = DetailBuilder.Make(planetGO, sector, mod, _standingVisionTorchPrefab, new DetailInfo(info));

            if (standingTorch == null)
            {
                NHLogger.LogWarning($"Tried to make a vision torch target but couldn't. Do you have the DLC installed?");
                return null;
            }

            // Set some required properties on the torch
            var mindSlideProjector = standingTorch.GetComponent<MindSlideProjector>();
            mindSlideProjector._mindProjectorImageEffect = SearchUtilities.Find("Player_Body/PlayerCamera").GetComponent<MindProjectorImageEffect>();

            // Setup for visually supporting async texture loading
            mindSlideProjector.enabled = false;
            var visionBeamEffect = standingTorch.FindChild("VisionBeam");
            visionBeamEffect.SetActive(false);

            // Set up slides
            // The number of slides is unlimited, 16 is only for texturing the actual slide reel item. This is not a slide reel item
            var slides = info.slides;
            var slidesCount = slides.Length;
            var slideCollection = new SlideCollection(slidesCount);
            slideCollection.streamingAssetIdentifier = string.Empty; // NREs if null

            var (_, _, imageLoader) = StartAsyncLoader(mod, slides, ref slideCollection, false, false, true);

            // This variable just lets us track how many of the slides have been loaded.
            // This way as soon as the last one is loaded (due to async loading, this may be
            // slide 7, or slide 3, or whatever), we can enable the vision torch. This allows us
            // to avoid doing a "is every element in the array `slideCollection.slides` not null" check every time a texture finishes loading
            int displaySlidesLoaded = 0;
            imageLoader.imageLoadedEvent.AddListener(
                (Texture2D tex, int index, string originalPath) =>
                {
                    var time = DateTime.Now;
                    slideCollection.slides[index]._image = tex;

                    displaySlidesLoaded++;
                    if (displaySlidesLoaded == slides.Length)
                    {
                        mindSlideProjector.enabled = true;
                        visionBeamEffect.SetActive(true);
                    }
                    NHLogger.LogVerbose($"Slide reel another set time {(DateTime.Now - time).TotalMilliseconds}ms");
                }
            );

            // Set up the containers for the slides
            var slideCollectionContainer = standingTorch.AddComponent<NHSlideCollectionContainer>();
            slideCollectionContainer.doAsyncLoading = false;
            slideCollectionContainer.slideCollection = slideCollection;

            var mindSlideCollection = standingTorch.AddComponent<MindSlideCollection>();
            mindSlideCollection._slideCollectionContainer = slideCollectionContainer;

            // Make sure that these slides play when the player wanders into the beam
            slideCollectionContainer._initialized = true; // Hack to avoid initialization in the following call (it would throw NRE)
            mindSlideProjector.SetMindSlideCollection(mindSlideCollection);
            slideCollectionContainer._initialized = false;


            LinkShipLogFacts(info, slideCollectionContainer);

            standingTorch.SetActive(true);

            return standingTorch;
        }

        /// <summary>
        /// start loading all the slide stuff we need async.
        /// </summary>
        /// <param name="mod">the mod to load slides from</param>
        /// <param name="slides">slides to load</param>
        /// <param name="slideCollection">where to assign the slide objects</param>
        /// <param name="useInvertedCache">should we load cached inverted images?</param>
        /// <param name="useAtlasCache">should we load cached atlas images?</param>
        /// <param name="loadRawImages">should we load the original images? happens anyway if cache doesnt exist since atlas or inverted will need it</param>
        /// <returns>the 3 loaders (inverted, atlas, original). inverted and atlas will be null if cache doesnt exist, so check those to find out if cache exists</returns>
        private static (SlideReelAsyncImageLoader inverted, SlideReelAsyncImageLoader atlas, SlideReelAsyncImageLoader slides)
            StartAsyncLoader(IModBehaviour mod, SlideInfo[] slides, ref SlideCollection slideCollection, bool useInvertedCache, bool useAtlasCache, bool loadRawImages)
        {
            var invertedImageLoader = new SlideReelAsyncImageLoader();
            var atlasImageLoader = new SlideReelAsyncImageLoader();
            var imageLoader = new SlideReelAsyncImageLoader();

            var atlasKey = GetUniqueSlideReelID(mod, slides);

            var cacheExists = CacheExists(mod);

            NHLogger.Log($"Does cache exist for slide reels? {cacheExists}");

            if (useAtlasCache && cacheExists)
            {
                NHLogger.LogVerbose($"The atlas cache for slide reel containing [{slides.FirstOrDefault(x => !string.IsNullOrEmpty(x.imagePath))?.imagePath}] is {atlasKey}");
                // Load the atlas texture used to draw onto the physical slide reel object
                atlasImageLoader.PathsToLoad.Add((0, Path.Combine(mod.ModHelper.Manifest.ModFolderPath, AtlasSlideReelCacheFolder, $"{atlasKey}.png")));
            }

            for (int i = 0; i < slides.Length; i++)
            {
                var slide = new Slide();
                var slideInfo = slides[i];
                slide._streamingImageID = i; // for SlideRotationModule

                if (string.IsNullOrEmpty(slideInfo.imagePath))
                {
                    if (useInvertedCache && cacheExists)
                    {
                        // Load the inverted images used when displaying slide reels to a screen
                        invertedImageLoader.PathsToLoad.Add((i, Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, "Assets/textures/inverted_blank_slide_reel.png")));
                    }
                    else if (!cacheExists || loadRawImages)
                    {
                        // Used to then make cached stuff
                        imageLoader.PathsToLoad.Add((i, Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, "Assets/textures/blank_slide_reel.png")));
                    }
                }
                else
                {
                    if (cacheExists && useInvertedCache)
                    {
                        // Load the inverted images used when displaying slide reels to a screen
                        invertedImageLoader.PathsToLoad.Add((i, Path.Combine(mod.ModHelper.Manifest.ModFolderPath, InvertedSlideReelCacheFolder, slideInfo.imagePath)));
                    }
                    if (!cacheExists || loadRawImages)
                    {
                        imageLoader.PathsToLoad.Add((i, Path.Combine(mod.ModHelper.Manifest.ModFolderPath, slideInfo.imagePath)));
                    }
                }

                AddModules(slideInfo, ref slide, mod);

                slideCollection.slides[i] = slide;
            }

            if (cacheExists)
            {
                NHLogger.Log("Loading slide reels from cache");

                if (useAtlasCache)
                {
                    atlasImageLoader.Start(false, false);
                }
                if (useInvertedCache)
                {
                    invertedImageLoader.Start(true, false);
                }
                if (loadRawImages)
                {
                    imageLoader.Start(true, false);
                }

                return (invertedImageLoader, atlasImageLoader, imageLoader);
            }
            else
            {
                NHLogger.Log("Generating slide reel cache");

                // Will be slow and create the cache if needed
                // Will run sequentially to ensure we don't run out of memory
                imageLoader.Start(true, true);

                return (null, null, imageLoader);
            }
        }

        private static void AddModules(SlideInfo slideInfo, ref Slide slide, IModBehaviour mod)
        {
            var modules = new List<SlideFunctionModule>();
            if (!string.IsNullOrEmpty(slideInfo.beatAudio))
            {
                var audioBeat = new SlideBeatAudioModule
                {
                    _audioType = AudioTypeHandler.GetAudioType(slideInfo.beatAudio, mod),
                    _delay = slideInfo.beatDelay
                };
                modules.Add(audioBeat);
            }
            if (!string.IsNullOrEmpty(slideInfo.backdropAudio))
            {
                var audioBackdrop = new SlideBackdropAudioModule
                {
                    _audioType = AudioTypeHandler.GetAudioType(slideInfo.backdropAudio, mod),
                    _fadeTime = slideInfo.backdropFadeTime
                };
                modules.Add(audioBackdrop);
            }
            if (slideInfo.ambientLightIntensity != 0)
            {
                var ambientLight = new SlideAmbientLightModule
                {
                    _intensity = slideInfo.ambientLightIntensity,
                    _range = slideInfo.ambientLightRange,
                    _color = slideInfo.ambientLightColor?.ToColor() ?? Color.white,
                    _spotIntensityMod = slideInfo.spotIntensityMod
                };
                modules.Add(ambientLight);
            }
            if (slideInfo.playTimeDuration != 0)
            {
                var playTime = new SlidePlayTimeModule
                {
                    _duration = slideInfo.playTimeDuration
                };
                modules.Add(playTime);
            }
            if (slideInfo.blackFrameDuration != 0)
            {
                var blackFrame = new SlideBlackFrameModule
                {
                    _duration = slideInfo.blackFrameDuration
                };
                modules.Add(blackFrame);
            }
            if (!string.IsNullOrEmpty(slideInfo.reveal))
            {
                var shipLogEntry = new SlideShipLogEntryModule
                {
                    _entryKey = slideInfo.reveal
                };
                modules.Add(shipLogEntry);
            }
            if (slideInfo.rotate)
            {
                modules.Add(new SlideRotationModule());
            }

            Slide.WriteModules(modules, ref slide._modulesList, ref slide._modulesData, ref slide.lengths);
        }

        private static void LinkShipLogFacts(ProjectionInfo info, NHSlideCollectionContainer slideCollectionContainer)
        {
            // Idk why but it wants reveals to be comma delimited not a list
            if (info.reveals != null) slideCollectionContainer._shipLogOnComplete = string.Join(",", info.reveals);
            // Don't use null value, NRE in SlideCollectionContainer.Initialize
            slideCollectionContainer._playWithShipLogFacts = info.playWithShipLogFacts ?? Array.Empty<string>();

            slideCollectionContainer.conditionsToSet = info.conditionsToSet;
            slideCollectionContainer.persistentConditionsToSet = info.persistentConditionsToSet;
        }
    }

    public class VisionTorchTarget : MonoBehaviour
    {
        public MindSlideCollection slideCollection;

        // This Callback is never used in NH itself.
        // It exists for addons that want to trigger events when the mind slide show starts.
        public OWEvent.OWCallback onSlidesStart;

        // This Callback is never used in NH itself.
        // It exists for addons that want to trigger events after the mind slide show is complete.
        public OWEvent.OWCallback onSlidesComplete;
    }
}
