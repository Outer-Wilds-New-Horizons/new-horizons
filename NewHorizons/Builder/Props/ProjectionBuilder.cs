using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using static NewHorizons.External.Modules.PropModule;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.Props
{
    public static class ProjectionBuilder
    {
        private static GameObject _slideReelPrefab;
        private static GameObject _autoPrefab;
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");

        public static void Make(GameObject go, Sector sector, PropModule.ProjectionInfo info, IModBehaviour mod)
        {
            switch (info.type)
            {
                case PropModule.ProjectionInfo.SlideShowType.AutoProjector:
                    MakeAutoProjector(go, sector, info, mod);
                    break;
                case PropModule.ProjectionInfo.SlideShowType.SlideReel:
                    MakeSlideReel(go, sector, info, mod);
                    break;
                case PropModule.ProjectionInfo.SlideShowType.VisionTorchTarget:
                    MakeMindSlidesTarget(go, sector, info, mod);
                    break;
                case PropModule.ProjectionInfo.SlideShowType.StandingVisionTorch:
                    MakeStandingVisionTorch(go, sector, info, mod);
                    break;
                default:
                    Logger.LogError($"Invalid projection type {info.type}");
                    break;
            }
        }

        private static void MakeSlideReel(GameObject planetGO, Sector sector, PropModule.ProjectionInfo info, IModBehaviour mod)
        {
            if (_slideReelPrefab == null)
            {
                _slideReelPrefab = SearchUtilities.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone1/Sector_SlideBurningRoom_Zone1/Interactables_SlideBurningRoom_Zone1/Prefab_IP_SecretAlcove/RotationPivot/SlideReelSocket/Prefab_IP_Reel_1_LibraryPath")?.gameObject?.InstantiateInactive();
                if (_slideReelPrefab == null)
                {
                    Logger.LogWarning($"Tried to make a slide reel but couldn't. Do you have the DLC installed?");
                    return;
                }
                _slideReelPrefab.name = "Prefab_IP_Reel";
            }

            var slideReelObj = _slideReelPrefab.InstantiateInactive();
            slideReelObj.name = $"Prefab_IP_Reel_{mod.ModHelper.Manifest.Name}";

            var slideReel = slideReelObj.GetComponent<SlideReelItem>();
            slideReel.SetSector(sector);
            slideReel.SetVisible(true);

            var slideCollectionContainer = slideReelObj.GetRequiredComponent<SlideCollectionContainer>();

            foreach (var renderer in slideReelObj.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }

            slideReelObj.transform.parent = sector?.transform ?? planetGO.transform;
            slideReelObj.transform.position = planetGO.transform.TransformPoint((Vector3)(info.position ?? Vector3.zero));
            slideReelObj.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler((Vector3)(info.rotation ?? Vector3.zero)));

            // Now we replace the slides
            int slidesCount = info.slides.Length;
            var slideCollection = new SlideCollection(slidesCount);

            // The base game ones only have 15 slides max
            var textures = new Texture2D[slidesCount >= 15 ? 15 : slidesCount];

            var imageLoader = slideReelObj.AddComponent<ImageUtilities.AsyncImageLoader>();
            for (int i = 0; i < slidesCount; i++)
            {
                var slide = new Slide();
                var slideInfo = info.slides[i];

                imageLoader.pathsToLoad.Add(mod.ModHelper.Manifest.ModFolderPath + slideInfo.imagePath);

                AddModules(slideInfo, ref slide);

                slideCollection.slides[i] = slide;
            }
            
            // this variable just lets us track how many of the first 15 slides have been loaded.
            // this way as soon as the last one is loaded (due to async loading, this may be
            // slide 7, or slide 3, or whatever), we can build the slide reel texture. This allows us
            // to avoid doing a "is every element in the array `textures` not null" check every time a texture finishes loading
            int displaySlidesLoaded = 0;
            imageLoader.imageLoadedEvent.AddListener(
                (Texture2D tex, int index) => 
                { 
                    slideCollection.slides[index].textureOverride = ImageUtilities.Invert(tex); 

                    // Track the first 15 to put on the slide reel object
                    if (index < 15) 
                    {
                        textures[index] = tex;
                        displaySlidesLoaded++; // threading moment
                    }

                    if (displaySlidesLoaded >= textures.Length)
                    {
                        // all textures required to build the reel's textures have been loaded
                        var slidesBack = slideReelObj.transform.Find("Props_IP_SlideReel_7/Slides_Back").GetComponent<MeshRenderer>();
                        var slidesFront = slideReelObj.transform.Find("Props_IP_SlideReel_7/Slides_Front").GetComponent<MeshRenderer>();

                        // Now put together the textures into a 4x4 thing for the materials
                        var reelTexture = ImageUtilities.MakeReelTexture(textures);
                        slidesBack.material.mainTexture = reelTexture;
                        slidesBack.material.SetTexture(EmissionMap, reelTexture);
                        slidesFront.material.mainTexture = reelTexture;
                        slidesFront.material.SetTexture(EmissionMap, reelTexture);
                    }
                }
            );

            // Else when you put them down you can't pick them back up
            slideReelObj.GetComponent<OWCollider>()._physicsRemoved = false;

            slideCollectionContainer.slideCollection = slideCollection;

            // Idk why but it wants reveals to be comma delimited not a list
            if (info.reveals != null) slideCollectionContainer._shipLogOnComplete = string.Join(",", info.reveals);

            OWAssetHandler.LoadObject(slideReelObj);
            sector.OnOccupantEnterSector.AddListener((x) => OWAssetHandler.LoadObject(slideReelObj));

            slideReelObj.SetActive(true);
        }

        public static void MakeAutoProjector(GameObject planetGO, Sector sector, PropModule.ProjectionInfo info, IModBehaviour mod)
        {
            if (_autoPrefab == null)
            {
                _autoPrefab = SearchUtilities.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone4/Sector_BlightedShore/Sector_JammingControlRoom_Zone4/Interactables_JammingControlRoom_Zone4/AutoProjector_SignalJammer/Prefab_IP_AutoProjector_SignalJammer")?.gameObject?.InstantiateInactive();
                if (_autoPrefab == null)
                {
                    Logger.LogWarning($"Tried to make a auto projector but couldn't. Do you have the DLC installed?");
                    return;
                }
                _autoPrefab.name = "Prefab_IP_AutoProjector";
            }

            var projectorObj = _autoPrefab.InstantiateInactive();
            projectorObj.name = $"Prefab_IP_AutoProjector_{mod.ModHelper.Manifest.Name}";

            var autoProjector = projectorObj.GetComponent<AutoSlideProjector>();
            autoProjector._sector = sector;

            var slideCollectionContainer = autoProjector.GetRequiredComponent<SlideCollectionContainer>();

            autoProjector.transform.parent = sector?.transform ?? planetGO.transform;
            autoProjector.transform.position = planetGO.transform.TransformPoint((Vector3)(info.position ?? Vector3.zero));
            autoProjector.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler((Vector3)(info.rotation ?? Vector3.zero)));

            // Now we replace the slides
            int slidesCount = info.slides.Length;
            var slideCollection = new SlideCollection(slidesCount);
            
            var imageLoader = projectorObj.AddComponent<ImageUtilities.AsyncImageLoader>();
            for (int i = 0; i < slidesCount; i++)
            {
                var slide = new Slide();
                var slideInfo = info.slides[i];

                imageLoader.pathsToLoad.Add(mod.ModHelper.Manifest.ModFolderPath + slideInfo.imagePath);

                AddModules(slideInfo, ref slide);

                slideCollection.slides[i] = slide;
            }
            imageLoader.imageLoadedEvent.AddListener((Texture2D tex, int index) => { slideCollection.slides[index].textureOverride = ImageUtilities.Invert(tex); });

            slideCollectionContainer.slideCollection = slideCollection;

            OWAssetHandler.LoadObject(projectorObj);
            sector.OnOccupantEnterSector.AddListener((x) => OWAssetHandler.LoadObject(projectorObj));

            // Change the picture on the lens
            var lens = projectorObj.transform.Find("Spotlight/Prop_IP_SingleSlideProjector/Projector_Lens").GetComponent<MeshRenderer>();
            lens.materials[1].mainTexture = slideCollection.slides[0]._textureOverride;
            lens.materials[1].SetTexture(EmissionMap, slideCollection.slides[0]._textureOverride);

            projectorObj.SetActive(true);
        }

        // Makes a target for a vision torch to scan
        public static GameObject MakeMindSlidesTarget(GameObject planetGO, Sector sector, PropModule.ProjectionInfo info, IModBehaviour mod)
        {
            // spawn a trigger for the vision torch
            var path = "DreamWorld_Body/Sector_DreamWorld/Sector_Underground/Sector_PrisonCell/Ghosts_PrisonCell/GhostNodeMap_PrisonCell_Lower/Prefab_IP_GhostBird_Prisoner/Ghostbird_IP_ANIM/Ghostbird_Skin_01:Ghostbird_Rig_V01:Base/Ghostbird_Skin_01:Ghostbird_Rig_V01:Root/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine01/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine02/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine03/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine04/Ghostbird_Skin_01:Ghostbird_Rig_V01:Neck01/Ghostbird_Skin_01:Ghostbird_Rig_V01:Neck02/Ghostbird_Skin_01:Ghostbird_Rig_V01:Head/PrisonerHeadDetector";
            var g = DetailBuilder.MakeDetail(planetGO, sector, path, info.position, Vector3.zero, 2, false);

            if (g == null)
            {
                Logger.LogWarning($"Tried to make a vision torch target but couldn't. Do you have the DLC installed?");
                return null;
            }

            g.name = "VisionStaffDetector";

            // The number of slides is unlimited, 15 is only for texturing the actual slide reel item. This is not a slide reel item
            var slides = info.slides;
            var slidesCount = slides.Length;
            var slideCollection = new SlideCollection(slidesCount);

        
            var imageLoader = g.AddComponent<ImageUtilities.AsyncImageLoader>();
            for (int i = 0; i < slidesCount; i++)
            {
                var slide = new Slide();
                var slideInfo = slides[i];

                imageLoader.pathsToLoad.Add(mod.ModHelper.Manifest.ModFolderPath + slideInfo.imagePath);

                AddModules(slideInfo, ref slide);

                slideCollection.slides[i] = slide;
            }
            imageLoader.imageLoadedEvent.AddListener((Texture2D tex, int index) => { slideCollection.slides[index].textureOverride = tex; });


            // attatch a component to store all the data for the slides that play when a vision torch scans this target
            var target = g.AddComponent<VisionTorchTarget>();
            var slideCollectionContainer = g.AddComponent<SlideCollectionContainer>();
            slideCollectionContainer.slideCollection = slideCollection;
            target.slideCollection = g.AddComponent<MindSlideCollection>();
            target.slideCollection._slideCollectionContainer = slideCollectionContainer;
            target.slideCollectionContainer = slideCollectionContainer;

            // Idk why but it wants reveals to be comma delimited not a list
            if (info.reveals != null) slideCollectionContainer._shipLogOnComplete = string.Join(",", info.reveals);

            return g;
        }

        public static GameObject MakeStandingVisionTorch(GameObject planetGO, Sector sector, PropModule.ProjectionInfo info, IModBehaviour mod)
        {
            //
            // spawn the torch itself
            //

            var path = "RingWorld_Body/Sector_RingWorld/Sector_SecretEntrance/Interactibles_SecretEntrance/Experiment_1/VisionTorchApparatus/VisionTorchRoot/Prefab_IP_VisionTorchProjector";
            var standingTorch = DetailBuilder.MakeDetail(planetGO, sector, path, info.position, info.rotation, 1, false);

            if (standingTorch == null)
            {
                Logger.LogWarning($"Tried to make a vision torch target but couldn't. Do you have the DLC installed?");
                return null;
            }

            //
            // set some required properties on the torch
            //

            var mindSlideProjector = standingTorch.GetComponent<MindSlideProjector>();
            mindSlideProjector._mindProjectorImageEffect = SearchUtilities.Find("Player_Body/PlayerCamera").GetComponent<MindProjectorImageEffect>();
            
            // setup for visually supporting async texture loading
            mindSlideProjector.enabled = false;	
            var visionBeamEffect = SearchUtilities.FindChild(standingTorch, "VisionBeam");
            visionBeamEffect.SetActive(false);

            //
            // set up slides
            //

            // The number of slides is unlimited, 15 is only for texturing the actual slide reel item. This is not a slide reel item
            var slides = info.slides;
            var slidesCount = slides.Length;
            var slideCollection = new SlideCollection(slidesCount);

            var imageLoader = standingTorch.AddComponent<ImageUtilities.AsyncImageLoader>();
            for (int i = 0; i < slidesCount; i++)
            {
                var slide = new Slide();
                var slideInfo = slides[i];

                imageLoader.pathsToLoad.Add(mod.ModHelper.Manifest.ModFolderPath + slideInfo.imagePath);

                AddModules(slideInfo, ref slide);

                slideCollection.slides[i] = slide;
            }
            
            // this variable just lets us track how many of the slides have been loaded.
            // this way as soon as the last one is loaded (due to async loading, this may be
            // slide 7, or slide 3, or whatever), we can enable the vision torch. This allows us
            // to avoid doing a "is every element in the array `slideCollection.slides` not null" check every time a texture finishes loading
            int displaySlidesLoaded = 0;
            imageLoader.imageLoadedEvent.AddListener(
                (Texture2D tex, int index) => 
                { 
                    slideCollection.slides[index].textureOverride = tex;
                    displaySlidesLoaded++; // threading moment

                    if (displaySlidesLoaded >= slides.Length)
                    {
                        mindSlideProjector.enabled = true;
                        visionBeamEffect.SetActive(true);
                    }
                }
            );

            // set up the containers for the slides
            var slideCollectionContainer = standingTorch.AddComponent<SlideCollectionContainer>();
            slideCollectionContainer.slideCollection = slideCollection;
            var mindSlideCollection = standingTorch.AddComponent<MindSlideCollection>();
            mindSlideCollection._slideCollectionContainer = slideCollectionContainer;

            // make sure that these slides play when the player wanders into the beam
            // _slideCollectionItem is actually a reference to a SlideCollectionContainer. Not a slide reel item
            mindSlideProjector._mindSlideCollection = mindSlideCollection;
            mindSlideProjector._slideCollectionItem = slideCollectionContainer; 
            mindSlideProjector.SetMindSlideCollection(mindSlideCollection);


            // Idk why but it wants reveals to be comma delimited not a list
            if (info.reveals != null) slideCollectionContainer._shipLogOnComplete = string.Join(",", info.reveals);

            return standingTorch;
        }

        private static void AddModules(PropModule.SlideInfo slideInfo, ref Slide slide)
        {
            var modules = new List<SlideFunctionModule>();
            if (!String.IsNullOrEmpty(slideInfo.beatAudio))
            {
                var audioBeat = new SlideBeatAudioModule();
                audioBeat._audioType = (AudioType)Enum.Parse(typeof(AudioType), slideInfo.beatAudio);
                audioBeat._delay = slideInfo.beatDelay;
                modules.Add(audioBeat);
            }
            if (!String.IsNullOrEmpty(slideInfo.backdropAudio))
            {
                var audioBackdrop = new SlideBackdropAudioModule();
                audioBackdrop._audioType = (AudioType)Enum.Parse(typeof(AudioType), slideInfo.backdropAudio);
                audioBackdrop._fadeTime = slideInfo.backdropFadeTime;
                modules.Add(audioBackdrop);
            }
            if (slideInfo.ambientLightIntensity > 0)
            {
                var ambientLight = new SlideAmbientLightModule();
                ambientLight._intensity = slideInfo.ambientLightIntensity;
                ambientLight._range = slideInfo.ambientLightRange;
                ambientLight._color = slideInfo.ambientLightColor.ToColor();
                ambientLight._spotIntensityMod = slideInfo.spotIntensityMod;
                modules.Add(ambientLight);
            }
            if (slideInfo.playTimeDuration != 0)
            {
                var playTime = new SlidePlayTimeModule();
                playTime._duration = slideInfo.playTimeDuration;
                modules.Add(playTime);
            }
            if (slideInfo.blackFrameDuration != 0)
            {
                var blackFrame = new SlideBlackFrameModule();
                blackFrame._duration = slideInfo.blackFrameDuration;
                modules.Add(blackFrame);
            }
            if (!String.IsNullOrEmpty(slideInfo.reveal))
            {
                var shipLogEntry = new SlideShipLogEntryModule();
                shipLogEntry._entryKey = slideInfo.reveal;
                modules.Add(shipLogEntry);
            }

            Slide.WriteModules(modules, ref slide._modulesList, ref slide._modulesData, ref slide.lengths);
        }
    }

    public class VisionTorchTarget : MonoBehaviour
    {
        public MindSlideCollection slideCollection;
        public SlideCollectionContainer slideCollectionContainer;
        public OWEvent.OWCallback onSlidesComplete;
    }
}
