using HarmonyLib;
using NewHorizons.Components.EOTE;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(Peephole))]
    public static class PeepholePatches
    {
        static List<Sector> _previousSectors = new List<Sector>();

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Peephole.SwitchToPeepholeCamera))]
        public static void Peephole_SwitchToPeepholeCamera_Prefix()
        {
            _previousSectors.Clear();
            _previousSectors.AddRange(Locator.GetPlayerSectorDetector()._sectorList);
        }


        [HarmonyPostfix]
        [HarmonyPatch(nameof(Peephole.SwitchToPeepholeCamera))]
        public static void Peephole_SwitchToPeepholeCamera(Peephole __instance)
        {
            if (__instance._viewingSector)
            {
                var sector = __instance._viewingSector;
                while (sector._parentSector != null)
                {
                    sector = sector._parentSector;

                    if (!_previousSectors.Contains(sector))
                    {
                        sector.AddOccupant(Locator.GetPlayerSectorDetector());
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Peephole.SwitchToPlayerCamera))]
        public static void Peephole_SwitchToPlayerCamera(Peephole __instance)
        {
            if (__instance._viewingSector)
            {
                var sector = __instance._viewingSector;

                if (_previousSectors.Contains(sector))
                {
                    sector.AddOccupant(Locator.GetPlayerSectorDetector());
                }

                while (sector._parentSector != null)
                {
                    sector = sector._parentSector;

                    if (!_previousSectors.Contains(sector))
                    {
                        sector.RemoveOccupant(Locator.GetPlayerSectorDetector());
                    }
                }
            }

            _previousSectors.Clear();
        }
    }
}
