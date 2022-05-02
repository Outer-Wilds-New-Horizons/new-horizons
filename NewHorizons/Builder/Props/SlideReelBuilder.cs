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
                _prefab = GameObject.FindObjectOfType<SlideReelItem>().gameObject.InstantiateInactive();
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
            int slidesCount = info.slideImagePaths.Length;
            var slideCollection = new SlideCollection(slidesCount);

            for(int i = 0; i < slidesCount; i++)
            {
                var slide = new Slide();
                slide.textureOverride = ImageUtilities.GetTextureForSlide(mod, info.slideImagePaths[i]);
                slideCollection.slides[i] = slide;
            }

            slideCollectionContainer.slideCollection = slideCollection;
            if (info.reveals != null) slideCollectionContainer._shipLogOnComplete = string.Join(",", info.reveals);

            OWAssetHandler.LoadObject(slideReelObj);

            slideReelObj.SetActive(true);
        }
    }
}
