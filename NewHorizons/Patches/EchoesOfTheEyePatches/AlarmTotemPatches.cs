using HarmonyLib;
using NewHorizons.Components.EOTE;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(AlarmTotem))]
    public static class AlarmTotemPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(AlarmTotem.SetFaceOpen))]
        public static void AlarmTotem_SetFaceOpen(AlarmTotem __instance, bool open)
        {
            // This method is unused in the base game and sets the rotations incorrectly (-90f instead of 90f); this corrects that
            __instance._rightFaceCover.localEulerAngles = Vector3.up * (open ? 90f : 0f);
            __instance._leftFaceCover.localEulerAngles = Vector3.up * (open ? -90f : 0f);
        }
    }
}
