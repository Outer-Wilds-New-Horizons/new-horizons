using NewHorizons.Builder.General;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HarmonyLib;
using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Object = UnityEngine.Object;
using NewHorizons.Handlers;
using NewHorizons.Builder.ShipLog;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class PlayerSpawnerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerSpawner), nameof(PlayerSpawner.SpawnPlayer))]
        public static void PlayerSpawner_SpawnPlayer(PlayerSpawner __instance)
        {
            Logger.Log("Player spawning");
            __instance.SetInitialSpawnPoint(Main.SystemDict[Main.Instance.CurrentStarSystem].SpawnPoint);
        }
    }
}
