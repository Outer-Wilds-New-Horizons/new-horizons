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
			VisionTorchTarget t = hitObj.GetComponent<VisionTorchTarget>();
            if (t != null) //(hitObj.CompareTag("PrisonerDetector"))
		    {
                // _slideCollectionItem is actually a reference to a SlideCollectionContainer. Not a slide reel item
				__instance._mindProjector._slideCollectionItem = t.slideCollectionContainer; 
				__instance._mindProjector._mindSlideCollection = t.slideCollection;
                __instance._mindProjector.SetMindSlideCollection(t.slideCollection);

				Main.Instance.ModHelper.Console.WriteLine("MIND PROJECTOR CUSTOM TRIGGER");
			    __instance.OnBeamStartHitPrisoner.Invoke();
			    __instance._mindProjector.Play(reset: true);
			    __instance._mindProjector.OnProjectionStart += new OWEvent.OWCallback(__instance.OnProjectionStart);
			    __instance._mindProjector.OnProjectionComplete += new OWEvent.OWCallback(__instance.OnProjectionComplete);

          //      __instance._mindProjector._slideCollectionItem.onSlideTextureUpdated += new OWEvent.OWCallback(__instance._mindProjector.OnSlideTextureUpdated);
		        //__instance._mindProjector._slideCollectionItem.onPlayBeatAudio += new OWEvent<AudioType>.OWCallback(__instance._mindProjector.OnPlayBeatAudio);
		        //__instance._mindProjector._slideCollectionItem.Initialize();
         
			    Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(hitObj.transform, Vector3.zero);
			    __instance._playerLockedOn = true;
                return false;
            }

            return true;
        }

        // TOOD: OnTriggerVolumeExit
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

			return true;
		}

        // ProbeLauncher.Disable()?
        // public override void PickUpItem(Transform holdTranform)
	}
}
