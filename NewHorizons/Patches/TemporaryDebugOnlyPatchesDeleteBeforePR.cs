//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace NewHorizons.Patches
//{
//	[HarmonyPatch]
//	public static class TemporaryDebugOnlyPatchesDeleteBeforePR
//    {
//		// This is some dark magic
//		// this creates a method called base_DropItem that basically just calls OWItem.PickUpItem whenever it (VisionTorchItemPatches.base_PickUpItem) is called
//		[HarmonyReversePatch]
//		[HarmonyPatch(typeof(VisibilityObject), nameof(VisibilityObject.Awake))]
//		private static void base_Awake(VisibilityObject __instance) { }


//        // Make the vision torch droppable. In the base game you can only drop it if you're in the dream world.
//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(QuantumShuffleObject), nameof(QuantumShuffleObject.Awake))]
//		public static bool QuantumShuffleObject_Awake(QuantumShuffleObject __instance)
//		{
//		    base_Awake(__instance);
//		    __instance._indexList = new List<int>(__instance._shuffledObjects.Length);
//		    __instance._localPositions = new Vector3[__instance._shuffledObjects.Length];
//		    for (int i = 0; i < __instance._shuffledObjects.Length; i++)
//		    {
//			    __instance._localPositions[i] = __instance._shuffledObjects[i].localPosition;
//		    }

//			return false;
//		}


//     //   [HarmonyPrefix]
//     //   [HarmonyPatch(typeof(MultiStateQuantumObject), nameof(MultiStateQuantumObject.ChangeQuantumState))]
//     //   public static bool MultiStateQuantumObject_ChangeQuantumState(MultiStateQuantumObject __instance, bool skipInstantVisibilityCheck)
//	    //{
//     //       if (__instance.gameObject.name == "Quantum States - shuffle1") { NewHorizons.Utility.Logger.Log("changing state"); }
//		   // for (int i = 0; i < __instance._prerequisiteObjects.Length; i++)
//		   // {
//			  //  if (!__instance._prerequisiteObjects[i].HasCollapsed())
//			  //  {
//				 //   return false;
//			  //  }
//		   // }
//		   // int stateIndex = __instance._stateIndex;
//		   // if (__instance._stateIndex == -1 && __instance._initialState != -1)
//		   // {
//			  //  __instance._stateIndex = __instance._initialState;
//		   // }
//		   // else if (__instance._sequential)
//		   // {
//			  //  __instance._stateIndex = (__instance._reverse ? (__instance._stateIndex - 1) : (__instance._stateIndex + 1));
//			  //  if (__instance._loop)
//			  //  {
//				 //   if (__instance._stateIndex < 0)
//				 //   {
//					//    __instance._stateIndex = __instance._states.Length - 1;
//				 //   }
//				 //   else if (__instance._stateIndex > __instance._states.Length - 1)
//				 //   {
//					//    __instance._stateIndex = 0;
//				 //   }
//			  //  }
//			  //  else
//			  //  {
//				 //   __instance._stateIndex = Mathf.Clamp(__instance._stateIndex, 0, __instance._states.Length - 1);
//			  //  }
//		   // }
//		   // else
//		   // {
//			  //  int num = 0; // num = the sum of all probabilities of states other than the current
//			  //  for (int j = 0; j < __instance._states.Length; j++)
//			  //  {
//				 //   if (j != stateIndex)
//				 //   {
//					//    __instance._probabilities[j] = __instance._states[j].GetProbability();
//					//    num += __instance._probabilities[j];
//				 //   }
//			  //  }

//     //           // this function constructs a sort of segmented range:
//     //           // 0                   3     5  6
//     //           // +-------------------+-----+--+
//     //           // | state 1           | s2  |s3|
//     //           // +-------------------+-----+--+
//     //           //
//     //           // num is the max value of this range (min is always 0)
//     //           // num2 is a random point on this range
//     //           //
//     //           // num3 and num4 track the bounds of the current segment being considered
//     //           // num3 is the min value, num4 is the max. for example, if num3=5 then num4=6
//     //           //
//     //           // the for looop uses num3 and num4 to figure out which segment num2 landed in

//			  //  int num2 = UnityEngine.Random.Range(0, num); 
//			  //  int num3 = 0;
//			  //  int num4 = 0;
//     //           NewHorizons.Utility.Logger.Log("num2: " + num2 + "   num: " + num);
//			  //  for (int k = 0; k < __instance._states.Length; k++)
//			  //  {
//     //               NewHorizons.Utility.Logger.Log("checking out state " + k);
//				 //   if (k != stateIndex)
//				 //   {
//     //                   NewHorizons.Utility.Logger.Log("considering state " + k);
//					//    num3 = num4;
//					//    num4 += __instance._probabilities[k];
//     //                   NewHorizons.Utility.Logger.Log("num4: " + num4);
//					//    if (__instance._probabilities[k] > 0 && num2 >= num3 && num2 < num4)
//					//    {
//     //                       NewHorizons.Utility.Logger.Log("picking state " + k);
//					//	    __instance._stateIndex = k;
//					//	    break;
//					//    }
//				 //   }
//			  //  }
//		   // }

//     //       NewHorizons.Utility.Logger.Log("previous state index: " + stateIndex + "   new state index: " + __instance._stateIndex);

//		   // if (stateIndex != __instance._stateIndex && stateIndex >= 0 && stateIndex < __instance._states.Length)
//		   // {
//     //           NewHorizons.Utility.Logger.Log("setting previous state invisible");
//			  //  __instance._states[stateIndex].SetVisible(visible: false);
//		   // }
//     //       NewHorizons.Utility.Logger.Log($"states length {__instance._states.Length} index {__instance._stateIndex}");
            
//		   // __instance._states[__instance._stateIndex].SetVisible(visible: true);
//		   // if (__instance._sequential && !__instance._loop && __instance._stateIndex == __instance._states.Length - 1)
//		   // {
//     //           NewHorizons.Utility.Logger.Log("disabling quantum behavior");
//			  //  __instance.SetActivation(active: false);
//		   // }

//		   // return false;
//	    //}
//	}

//}
