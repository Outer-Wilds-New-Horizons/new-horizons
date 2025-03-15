using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(SingleLightSensor))]
    public static class SingleLightSensorPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SingleLightSensor.Start))]
        public static void Start(SingleLightSensor __instance)
        {
            // SingleLightSensor assumes that the sector will be empty when it starts and disables itself, but this may not be true if it starts disabled and is activated later, or spawned via the API
            if (__instance._sector && __instance._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
            {
                __instance.OnSectorOccupantsUpdated();
            }
        }
    }
}
