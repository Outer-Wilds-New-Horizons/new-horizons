using HarmonyLib;
using NewHorizons.Components.Quantum;

namespace NewHorizons.Patches.VolumePatches
{
    [HarmonyPatch(typeof(DestructionVolume))]
    public static class DestructionVolumePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(DestructionVolume.Vanish))]
        public static bool DestructionVolume_Vanish(OWRigidbody bodyToVanish)
        {
            var quantumPlanet = bodyToVanish.gameObject.GetComponent<QuantumPlanet>();

            if (quantumPlanet == null) return true;

            // Allow it to vanish if this is the only state
            if (quantumPlanet.states.Count <= 1) return true;

            // Force it to change states but if it can't, remove it
            var oldIndex = quantumPlanet.CurrentIndex;
            quantumPlanet.ChangeQuantumState(true);
            if (quantumPlanet.CurrentIndex == oldIndex) return true;

            quantumPlanet.states.RemoveAt(oldIndex);

            return false;
        }

        /// <summary>
        /// This method detects Nomai shuttles that are inactive
        /// When active, it swaps the position of the NomaiShuttleController and the Rigidbody, so its not found as a child here and explodes continuously forever
        /// Just ignore the shuttle if its inactive
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(DestructionVolume.VanishNomaiShuttle))]
        public static bool DestructionVolume_VanishNomaiShuttle(DestructionVolume __instance, OWRigidbody shuttleBody, RelativeLocationData entryLocation)
        {
            if (shuttleBody.GetComponentInChildren<NomaiShuttleController>() == null)
            {
                if (__instance._nomaiShuttleBody == shuttleBody)
                {
                    __instance._nomaiShuttleBody = null;
                }
                return false;
            }
            return true;
        }
    }
}
