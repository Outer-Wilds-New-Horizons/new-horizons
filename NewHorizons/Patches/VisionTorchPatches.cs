using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using NewHorizons.Builder.Props;
using UnityEngine;

namespace NewHorizons.Patches
{
	
    [HarmonyPatch]
    public static class MindProjectorTriggerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MindProjectorTrigger), nameof(MindProjectorTrigger.OnTriggerVolumeEntry))]
        public static bool MindProjectorTrigger_OnTriggerVolumeEntry(MindProjectorTrigger __instance, GameObject hitObj)
        {
			var t = hitObj.GetComponent<VisionTorchTarget>();
            if (t != null) //(hitObj.CompareTag("PrisonerDetector"))
            {
                __instance._mindProjector.OnProjectionStart += t.onSlidesStart;
                __instance._mindProjector.OnProjectionComplete += t.onSlidesComplete;
                __instance._mindProjector.SetMindSlideCollection(t.slideCollection);

			    __instance.OnBeamStartHitPrisoner.Invoke();
			    __instance._mindProjector.Play(reset: true);
			    __instance._mindProjector.OnProjectionStart += __instance.OnProjectionStart;
			    __instance._mindProjector.OnProjectionComplete += __instance.OnProjectionComplete;
         
			    Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(hitObj.transform, Vector3.zero);
			    __instance._playerLockedOn = true;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MindProjectorTrigger), nameof(MindProjectorTrigger.OnTriggerVolumeExit))]
        private static bool MindProjectorTrigger_OnTriggerVolumeExit(MindProjectorTrigger __instance, GameObject hitObj)
        {
            var t = hitObj.GetComponent<VisionTorchTarget>();
            if (t != null) //(hitObj.CompareTag("PrisonerDetector"))
            {
                __instance._mindProjector.OnProjectionStart -= t.onSlidesStart;
                __instance._mindProjector.OnProjectionComplete -= t.onSlidesComplete;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MindSlideProjector), nameof(MindSlideProjector.SetMindSlideCollection))]
        private static bool MindSlideProjector_SetMindSlideCollection(MindSlideProjector __instance, MindSlideCollection mindSlideCollection)
        {
            if (mindSlideCollection == null) return false;

            if (__instance._mindSlideCollection == mindSlideCollection) return false;

            // Original method didn't check if old _slideCollectionItem was null.
            if (__instance._slideCollectionItem != null)
            {
                __instance._slideCollectionItem.onSlideTextureUpdated -= __instance.OnSlideTextureUpdated;
                __instance._slideCollectionItem.onPlayBeatAudio -= __instance.OnPlayBeatAudio;
            }

            __instance._mindSlideCollection = mindSlideCollection;
            __instance._defaultSlideDuration = mindSlideCollection.defaultSlideDuration;

            __instance._slideCollectionItem = mindSlideCollection.slideCollectionContainer;
            __instance._slideCollectionItem.onSlideTextureUpdated += __instance.OnSlideTextureUpdated;
            __instance._slideCollectionItem.onPlayBeatAudio += __instance.OnPlayBeatAudio;
            __instance._slideCollectionItem.Initialize();
            __instance._slideCollectionItem.enabled = false;

            return false;
        }
    }

	[HarmonyPatch]
	public static class VisionTorchItemPatches
    {
		// This is some dark magic
		// this creates a method called base_DropItem that basically just calls OWItem.PickUpItem whenever it (VisionTorchItemPatches.base_PickUpItem) is called
		[HarmonyReversePatch]
		[HarmonyPatch(typeof(OWItem), nameof(OWItem.DropItem))]
		private static void base_DropItem(OWItem instance, Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget) { }


        // Make the vision torch droppable. In the base game you can only drop it if you're in the dream world.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(VisionTorchItem), nameof(VisionTorchItem.DropItem))]
		public static bool VisionTorchItem_DropItem(VisionTorchItem __instance, Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
		{
			if (!Locator.GetDreamWorldController().IsInDream())
			{
				base_DropItem(__instance, position, normal, parent, sector, customDropTarget);
			}

            if (__instance._wasProjecting) __instance._mindProjectorTrigger.SetProjectorActive(false);

            __instance.gameObject.GetComponent<Collider>().enabled = true;

			return true;
		}
	}
}
