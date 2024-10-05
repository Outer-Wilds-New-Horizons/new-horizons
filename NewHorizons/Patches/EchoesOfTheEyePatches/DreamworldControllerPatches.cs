using HarmonyLib;
using NewHorizons.Components.EOTE;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(DreamWorldController))]
    public static class DreamworldControllerPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(DreamWorldController.FixedUpdate))]
        [HarmonyPatch(nameof(DreamWorldController.SpawnInDreamWorld))]
        public static IEnumerable<CodeInstruction> DreamWorldController_SpawnInDreamworld(IEnumerable<CodeInstruction> instructions)
        {
            // Thank you vio very cool!
            // For some reason in Patch 13 they made it so the planetary fog controller is disabled in the Dreamworld
            // This broke Hazy Dreams
            return new CodeMatcher(instructions).MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(DreamWorldController), nameof(DreamWorldController._playerCamera))),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Property(typeof(OWCamera), nameof(OWCamera.planetaryFog)).GetGetMethod()),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Property(typeof(Behaviour), nameof(Behaviour.enabled)).GetSetMethod())
                )
                .Repeat(matcher => matcher.RemoveInstructions(5))
                .InstructionEnumeration();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DreamWorldController.EnterDreamWorld))]
        public static void DreamWorldController_EnterDreamWorld(DreamWorldController __instance, DreamCampfire dreamCampfire, DreamArrivalPoint arrivalPoint)
        {
            // If we are arriving in a custom dream dimension, activate it immediately
            var dimension = arrivalPoint.GetAttachedOWRigidbody().GetComponent<DreamDimension>();
            if (dimension != null)
            {
                dimension.SetActive(true);
            }

            // Make the body/sector/volume where the player 'wakes up' in the dream match the body where the arrival point is located if it isn't the vanilla DreamWorld, or reset it if it is
            var dreamWorldAO = Locator.GetAstroObject(AstroObject.Name.DreamWorld);
            if (arrivalPoint.GetAttachedOWRigidbody() == dreamWorldAO.GetOWRigidbody())
            {
                __instance._dreamBody = dreamWorldAO.GetAttachedOWRigidbody();
                __instance._dreamWorldSector = dreamWorldAO.GetRootSector();
                __instance._dreamWorldVolume = __instance._dreamWorldSector.transform.Find("Volumes_DreamWorld").Find("DreamWorldVolume").GetComponent<OWTriggerVolume>();
            }
            else
            {
                var arrivalAO = arrivalPoint.GetComponentInParent<AstroObject>();
                __instance._dreamBody = arrivalAO.GetAttachedOWRigidbody();
                __instance._dreamWorldSector = arrivalAO.GetRootSector();
                __instance._dreamWorldVolume = arrivalAO._gravityVolume.GetOWTriggerVolume();
            }

            // Make the 'bubble' around the artifact load correctly when the destination body isn't the vanilla DreamWorld
            __instance._primarySimulationRoot.GetComponent<SectorCullGroup>().SetSector(__instance._dreamWorldSector);

            // Make the body where the player 'wakes up' out of the dream match the body where the campfire is located if it isn't the Stranger ("RingWorld"), or reset it if it is
            var ringWorldAO = Locator.GetAstroObject(AstroObject.Name.RingWorld);
            if (dreamCampfire.GetAttachedOWRigidbody() == ringWorldAO.GetOWRigidbody())
            {
                __instance._planetBody = ringWorldAO.GetAttachedOWRigidbody();
                __instance._ringWorldController = ringWorldAO.GetComponent<RingWorldController>();
            }
            else
            {
                var departureAO = dreamCampfire.GetComponentInParent<AstroObject>();
                __instance._planetBody = departureAO.GetAttachedOWRigidbody();
                __instance._ringWorldController = null;
            }
        }
    }
}
