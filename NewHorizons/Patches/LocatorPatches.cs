using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class LocatorPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Locator), nameof(Locator.RegisterCloakFieldController))]
        public static bool Locator_RegisterCloakFieldController()
        {
            return Locator._cloakFieldController == null;
        }
    }
}
