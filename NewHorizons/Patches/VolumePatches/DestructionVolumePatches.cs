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
    }
}
