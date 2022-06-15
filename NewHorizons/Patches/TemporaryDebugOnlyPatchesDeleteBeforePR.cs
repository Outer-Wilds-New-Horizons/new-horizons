using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Patches
{
	[HarmonyPatch]
	public static class TemporaryDebugOnlyPatchesDeleteBeforePR
    {
		// This is some dark magic
		// this creates a method called base_DropItem that basically just calls OWItem.PickUpItem whenever it (VisionTorchItemPatches.base_PickUpItem) is called
		[HarmonyReversePatch]
		[HarmonyPatch(typeof(QuantumObject), nameof(QuantumObject.Awake))]
		private static void base_Awake(QuantumObject __instance) { }


        // Make the vision torch droppable. In the base game you can only drop it if you're in the dream world.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuantumShuffleObject), nameof(QuantumShuffleObject.Awake))]
		public static bool QuantumShuffleObject_Awake(QuantumShuffleObject __instance)
		{
		    base_Awake(__instance);
		    __instance._indexList = new List<int>(__instance._shuffledObjects.Length);
		    __instance._localPositions = new Vector3[__instance._shuffledObjects.Length];
		    for (int i = 0; i < __instance._shuffledObjects.Length; i++)
		    {
			    __instance._localPositions[i] = __instance._shuffledObjects[i].localPosition;
		    }

			return false;
		}
	}

}
