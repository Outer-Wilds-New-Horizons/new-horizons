using HarmonyLib;
using NewHorizons.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class DestructionVolumePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DestructionVolume), nameof(DestructionVolume.Vanish))]
        public static bool DestructionVolume_Vanish(OWRigidbody __0)
        {
            var quantumPlanet = __0.gameObject.GetComponent<QuantumPlanet>();

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
