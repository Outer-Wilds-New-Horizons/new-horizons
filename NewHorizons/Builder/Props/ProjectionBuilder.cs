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
            if (info.type == "autoProjector") MakeAutoProjector(go, sector, info, mod);
            else if (info.type == "slideReel") MakeSlideReel(go, sector, info, mod);
            else if (info.type == "playerVisionTorchTarget") MakeMindSlidesTarget(go, sector, info, mod);
            else if (info.type == "standingVisionTorch") MakeStandingVisionTorch(go, sector, info, mod);
            else Logger.LogError($"Invalid projection type {info.type}");
        }

        private static void MakeSlideReel(GameObject planetGO, Sector sector, PropModule.ProjectionInfo info, IModBehaviour mod)
        {
            if (_slideReelPrefab == null)
            {
                _slideReelPrefab = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone1/Sector_SlideBurningRoom_Zone1/Interactables_SlideBurningRoom_Zone1/Prefab_IP_SecretAlcove/RotationPivot/SlideReelSocket/Prefab_IP_Reel_1_LibraryPath")?.gameObject?.InstantiateInactive();
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

            for (int i = 0; i < slidesCount; i++)
            {
                var slide = new Slide();
                var slideInfo = info.slides[i];

                var texture = ImageUtilities.GetTexture(mod, slideInfo.imagePath);
                slide.textureOverride = ImageUtilities.Invert(texture);

                // Track the first 15 to put on the slide reel object
                if (i < 15) textures[i] = texture;

                AddModules(slideInfo, ref slide);

                slideCollection.slides[i] = slide;
            }

            // Else when you put them down you can't pick them back up
            slideReelObj.GetComponent<OWCollider>()._physicsRemoved = false;

            slideCollectionContainer.slideCollection = slideCollection;

            // Idk why but it wants reveals to be comma delimited not a list
            if (info.reveals != null) slideCollectionContainer._shipLogOnComplete = string.Join(",", info.reveals);

            OWAssetHandler.LoadObject(slideReelObj);
            sector.OnOccupantEnterSector.AddListener((x) => OWAssetHandler.LoadObject(slideReelObj));

            var slidesBack = slideReelObj.transform.Find("Props_IP_SlideReel_7/Slides_Back").GetComponent<MeshRenderer>();
            var slidesFront = slideReelObj.transform.Find("Props_IP_SlideReel_7/Slides_Front").GetComponent<MeshRenderer>();

            // Now put together the textures into a 4x4 thing for the materials
            var reelTexture = ImageUtilities.MakeReelTexture(textures);
            slidesBack.material.mainTexture = reelTexture;
            slidesBack.material.SetTexture(EmissionMap, reelTexture);
            slidesFront.material.mainTexture = reelTexture;
            slidesFront.material.SetTexture(EmissionMap, reelTexture);

            slideReelObj.SetActive(true);
        }

        public static void MakeAutoProjector(GameObject planetGO, Sector sector, PropModule.ProjectionInfo info, IModBehaviour mod)
        {
            if (_autoPrefab == null)
            {
                _autoPrefab = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone4/Sector_BlightedShore/Sector_JammingControlRoom_Zone4/Interactables_JammingControlRoom_Zone4/AutoProjector_SignalJammer/Prefab_IP_AutoProjector_SignalJammer")?.gameObject?.InstantiateInactive();
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

            for (int i = 0; i < slidesCount; i++)
            {
                var slide = new Slide();
                var slideInfo = info.slides[i];

                var texture = ImageUtilities.GetTexture(mod, slideInfo.imagePath);
                slide.textureOverride = ImageUtilities.Invert(texture);

                AddModules(slideInfo, ref slide);

                slideCollection.slides[i] = slide;
            }

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


            for (int i = 0; i < slidesCount; i++)
            {
                var slide = new Slide();
                var slideInfo = slides[i];

                // TODO: do this part asynchronously so that you can load all the slides you want without stalling the game out for 5 days
                var texture = ImageUtilities.GetTexture(mod, slideInfo.imagePath);
                slide.textureOverride = texture; //ImageUtilities.Invert(texture);

                AddModules(slideInfo, ref slide);

                slideCollection.slides[i] = slide;
            }

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
			mindSlideProjector._mindProjectorImageEffect = GameObject.Find("Player_Body/PlayerCamera").GetComponent<MindProjectorImageEffect>();
			
            //
            // set up slides
            //

            // The number of slides is unlimited, 15 is only for texturing the actual slide reel item. This is not a slide reel item
            var slides = info.slides;
            var slidesCount = slides.Length;
            var slideCollection = new SlideCollection(slidesCount);

            for (int i = 0; i < slidesCount; i++)
            {
                var slide = new Slide();
                var slideInfo = slides[i];

                // TODO: do this part asynchronously so that you can load all the slides you want without stalling the game out for 5 days
                var texture = ImageUtilities.GetTexture(mod, slideInfo.imagePath);
                slide.textureOverride = texture; //ImageUtilities.Invert(texture);

                AddModules(slideInfo, ref slide);

                slideCollection.slides[i] = slide;
            }

            // set up the containers for the slides
            var slideCollectionContainer = standingTorch.AddComponent<SlideCollectionContainer>();
            slideCollectionContainer.slideCollection = slideCollection;
            var mindSlideCollection = standingTorch.AddComponent<MindSlideCollection>();
            mindSlideCollection._slideCollectionContainer = slideCollectionContainer;

            // make sure that these slides play when the player wanders into the beam
            // _slideCollectionItem is actually a reference to a SlideCollectionContainer. Not a slide reel item
            standingTorch.GetComponent<MindSlideProjector>()._mindSlideCollection = mindSlideCollection;
		    mindSlideProjector._slideCollectionItem = slideCollectionContainer; 
		    mindSlideProjector._mindSlideCollection = mindSlideCollection;
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
    }
}
