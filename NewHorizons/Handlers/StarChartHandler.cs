using Epic.OnlineServices;
using NewHorizons.Components.ShipLog;
using NewHorizons.External;
using NewHorizons.OtherMods.CustomShipLogModes;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class StarChartHandler
    {
        public static ShipLogStarChartMode ShipLogStarChartMode;

        private static Dictionary<string, string> _starSystemToFactID;
        private static Dictionary<string, string> _factIDToStarSystem;

        private static NewHorizonsSystem[] _systems;

        public static void Init(NewHorizonsSystem[] systems)
        {
            _systems = systems;

            var shipLogRoot = SearchUtilities.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas");
            var reticleImage = SearchUtilities.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ReticleImage (1)/");

            if (shipLogRoot != null && reticleImage != null)
            {
                var starChartLog = new GameObject("StarChartMode");
                starChartLog.SetActive(false);
                starChartLog.transform.parent = shipLogRoot.transform;
                starChartLog.transform.localScale = Vector3.one * 1f;
                starChartLog.transform.localPosition = Vector3.zero;
                starChartLog.transform.localRotation = Quaternion.Euler(0, 0, 0);

                ShipLogStarChartMode = starChartLog.AddComponent<ShipLogStarChartMode>();

                Object.Instantiate(reticleImage, starChartLog.transform);

                var scaleRoot = new GameObject("ScaleRoot");
                scaleRoot.transform.parent = starChartLog.transform;
                scaleRoot.transform.localScale = Vector3.one;
                scaleRoot.transform.localPosition = Vector3.zero;
                scaleRoot.transform.localRotation = Quaternion.Euler(0, 0, 0);

                var panRoot = new GameObject("PanRoot");
                panRoot.transform.parent = scaleRoot.transform;
                panRoot.transform.localScale = Vector3.one;
                panRoot.transform.localPosition = Vector3.zero;
                panRoot.transform.localRotation = Quaternion.Euler(0, 0, 0);

                CustomShipLogModesHandler.AddInterstellarMode();
            }

            _starSystemToFactID = new Dictionary<string, string>();
            _factIDToStarSystem = new Dictionary<string, string>();

            foreach (NewHorizonsSystem system in _systems)
            {
                if (system.Config.factRequiredForWarp != default && system.UniqueID != "SolarSystem")
                {
                    RegisterFactForSystem(system.Config.factRequiredForWarp, system.UniqueID);
                }

                if (system.UniqueID == Main.Instance.CurrentStarSystem && !string.IsNullOrEmpty(system.Config.factRequiredForWarpHome))
                {
                    RegisterFactForSystem(system.Config.factRequiredForWarpHome, "SolarSystem");
                }
            }
        }

        /// <summary>
        /// Can the player warp to any system at all
        /// </summary>
        /// <returns></returns>
        public static bool CanWarp()
        {
            foreach (var system in _systems)
            {
                if (CanWarpToSystem(system.UniqueID))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Do they have the fact required for a system
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public static bool HasUnlockedSystem(string system)
        {
            if (_starSystemToFactID == null || _starSystemToFactID.Count == 0)
                return true;

            // If we can't get a fact for the system, then its unlocked
            if (!_starSystemToFactID.TryGetValue(system, out var factID))
                return true;

            // It's unlocked if known
            return ShipLogHandler.KnowsFact(factID);
        }

        /// <summary>
        /// Is it actually a valid warp target
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public static bool CanWarpToSystem(string system)
        {
            var config = Main.SystemDict[system];

            var canWarpTo = false;
            if (system.Equals("SolarSystem")) canWarpTo = true;
            else if (system.Equals("EyeOfTheUniverse")) canWarpTo = false;
            else if (config.Spawn?.shipSpawn != null) canWarpTo = true;

            var canEnterViaWarpDrive = Main.SystemDict[system].Config.canEnterViaWarpDrive;

            // For the base solar system, use canWarpHome instead for better mod compat
            if (system.Equals("SolarSystem"))
            {
                canEnterViaWarpDrive = Main.SystemDict[Main.Instance.CurrentStarSystem].Config.canWarpHome;
            }

            return canWarpTo
                    && canEnterViaWarpDrive
                    && system != Main.Instance.CurrentStarSystem
                    && HasUnlockedSystem(system);
        }

        public static void OnRevealFact(string factID)
        {
            if (_factIDToStarSystem != null && _factIDToStarSystem.TryGetValue(factID, out var systemUnlocked))
            {
                NHLogger.Log($"Just learned [{factID}] and unlocked [{systemUnlocked}]");
                if (!Main.HasWarpDrive)
                    Main.Instance.EnableWarpDrive();
                if (ShipLogStarChartMode != null)
                    ShipLogStarChartMode.AddSystemCard(systemUnlocked);
            }
        }

        public static void RegisterFactForSystem(string factID, string system)
        {
            NHLogger.LogVerbose($"Need to know [{factID}] to unlock [{system}]");
            _starSystemToFactID.Add(system, factID);
            _factIDToStarSystem.Add(factID, system);
        }

        public static bool IsWarpDriveLockedOn() => StarChartHandler.ShipLogStarChartMode.GetTargetStarSystem() != null;
    }
}