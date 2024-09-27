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

        private static bool _canExitViaWarpDrive;
        private static string _factRequiredToExitViaWarpDrive;

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

                if (system.UniqueID == Main.Instance.CurrentStarSystem && !string.IsNullOrEmpty(system.Config.factRequiredToExitViaWarpDrive))
                {
                    _factRequiredToExitViaWarpDrive = system.Config.factRequiredToExitViaWarpDrive;
                    _canExitViaWarpDrive = system.Config.canExitViaWarpDrive || !string.IsNullOrEmpty(_factRequiredToExitViaWarpDrive);
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
            // If warp drive is entirely disabled, then no
            if (!CanExitViaWarpDrive())
                return false;

            if (_starSystemToFactID == null || _starSystemToFactID.Count == 0)
                return true;

            // If we can't get a fact for the system, then its unlocked
            if (!_starSystemToFactID.TryGetValue(system, out var factID))
                return true;

            // It's unlocked if known
            return ShipLogHandler.KnowsFact(factID);
        }

        public static bool CanExitViaWarpDrive() => _canExitViaWarpDrive
                && (string.IsNullOrEmpty(_factRequiredToExitViaWarpDrive) || ShipLogHandler.KnowsFact(_factRequiredToExitViaWarpDrive));

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

            var canEnterViaWarpDrive = Main.SystemDict[system].Config.canEnterViaWarpDrive || system == "SolarSystem";

            var canExitViaWarpDrive = CanExitViaWarpDrive();

            // Make base system always ignore canExitViaWarpDrive
            if (Main.Instance.CurrentStarSystem == "SolarSystem")
                canExitViaWarpDrive = true;

            NHLogger.Log(canEnterViaWarpDrive, canExitViaWarpDrive, system, HasUnlockedSystem(system));

            return canWarpTo
                    && canEnterViaWarpDrive
                    && canExitViaWarpDrive
                    && system != Main.Instance.CurrentStarSystem
                    && HasUnlockedSystem(system);
        }

        public static void OnRevealFact(string factID)
        {
            if (!string.IsNullOrEmpty(_factRequiredToExitViaWarpDrive) && factID == _factRequiredToExitViaWarpDrive)
            {
                if (!Main.HasWarpDrive)
                {
                    Main.Instance.EnableWarpDrive();
                    // Add all cards that now work
                    foreach (var starSystem in Main.SystemDict.Keys)
                    {
                        if (CanWarpToSystem(starSystem))
                        {
                            ShipLogStarChartMode.AddSystemCard(starSystem);
                        }
                    }
                }
                else
                {
                    NHLogger.LogWarning("Warp drive was enabled before learning fact?");
                }
            }

            if (_factIDToStarSystem != null && _factIDToStarSystem.TryGetValue(factID, out var systemUnlocked))
            {
                var knowsWarpFact = string.IsNullOrEmpty(_factRequiredToExitViaWarpDrive) || ShipLogHandler.KnowsFact(_factRequiredToExitViaWarpDrive);

                NHLogger.Log($"Just learned [{factID}] and unlocked [{systemUnlocked}]");
                if (!Main.HasWarpDrive && knowsWarpFact)
                {
                    Main.Instance.EnableWarpDrive();
                }
                ShipLogStarChartMode?.AddSystemCard(systemUnlocked);
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