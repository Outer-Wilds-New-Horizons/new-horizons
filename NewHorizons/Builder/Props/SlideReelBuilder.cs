using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.Utility;
using OWML.ModHelper;
using OWML.Common;
using NewHorizons.Handlers;

namespace NewHorizons.Builder.Props
{
    public static class SlideReelBuilder
    {
        private static GameObject _prefab;
        public static void Make(GameObject go, Sector sector, PropModule.SlideReelInfo info, IModBehaviour mod)
        {
            if (_prefab == null)
            {
                _prefab = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone1/Sector_SlideBurningRoom_Zone1/Interactables_SlideBurningRoom_Zone1/Prefab_IP_SecretAlcove/RotationPivot/SlideReelSocket/Prefab_IP_Reel_1_LibraryPath").gameObject.InstantiateInactive();
                _prefab.name = "Prefab_IP_Reel";
            }

            var slideReelObj = _prefab.InstantiateInactive();
            slideReelObj.name = $"Prefab_IP_Reel_{mod.ModHelper.Manifest.Name}";

            var slideReel = slideReelObj.GetComponent<SlideReelItem>();
            slideReel.SetSector(sector);
            slideReel.SetVisible(true);

            var slideCollectionContainer = slideReelObj.GetRequiredComponent<SlideCollectionContainer>();

            foreach(var renderer in slideReelObj.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }

            slideReelObj.transform.parent = sector?.transform ?? go.transform;
            slideReelObj.transform.localPosition = (Vector3)(info.position ?? Vector3.zero);
            slideReelObj.transform.rotation = Quaternion.Euler((Vector3)(info.position ?? Vector3.zero));

            // Now we replace the slides
            int slidesCount = info.slides.Length;
            var slideCollection = new SlideCollection(slidesCount);

            // The base game ones only have 15 slides max
            var textures = new Texture2D[slidesCount >= 15 ? 15 : slidesCount];

            for(int i = 0; i < slidesCount; i++)
            {
                var slide = new Slide();
                var slideInfo = info.slides[i];

                var texture = ImageUtilities.GetTexture(mod, slideInfo.imagePath);
                slide.textureOverride = ImageUtilities.Invert(texture);

                // Track the first 15 to put on the slide reel object
                if(i < 15) textures[i] = texture;

                AddModules(slideInfo, ref slide);

                slideCollection.slides[i] = slide;
            }

            // Else when you put them down you can't pick them back up
            slideReelObj.GetComponent<OWCollider>()._physicsRemoved = false;

            // Idk why but it wants reveals to be comma delimited not a list
            slideCollectionContainer.slideCollection = slideCollection;
            if (info.reveals != null) slideCollectionContainer._shipLogOnComplete = string.Join(",", info.reveals);

            OWAssetHandler.LoadObject(slideReelObj);

            var slidesBack = slideReelObj.transform.Find("Props_IP_SlideReel_7/Slides_Back").GetComponent<MeshRenderer>();
            var slidesFront = slideReelObj.transform.Find("Props_IP_SlideReel_7/Slides_Front").GetComponent<MeshRenderer>();

            // Now put together the textures into a 4x4 thing for the materials
            var reelTexture = ImageUtilities.MakeReelTexture(textures);
            slidesBack.material.mainTexture = reelTexture;
            slidesBack.material.SetTexture("_EmissionMap", reelTexture);
            slidesFront.material.mainTexture = reelTexture;
            slidesFront.material.SetTexture("_EmissionMap", reelTexture);

            slideReelObj.SetActive(true);
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
}
