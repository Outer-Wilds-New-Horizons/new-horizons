using HarmonyLib;
using UnityEngine;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
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
