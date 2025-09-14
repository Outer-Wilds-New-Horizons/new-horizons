using NewHorizons.Components.ShipLog;
using NewHorizons.External;
using NewHorizons.OtherMods.CustomShipLogModes;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class StarChartHandler
    {
        public static IShipLogStarChartMode CurrentMode => Main.UseLegacyStarChart ? ShipLogLegacyStarChartMode : ShipLogStarChartMode;
        public static ShipLogStarChartMode ShipLogStarChartMode;
        public static ShipLogLegacyStarChartMode ShipLogLegacyStarChartMode;

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

                var starChartReticle = Object.Instantiate(reticleImage, starChartLog.transform);
                starChartReticle.name = "ReticleImage";

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

                var starChartLogLegacy = Object.Instantiate(starChartLog, shipLogRoot.transform);
                starChartLogLegacy.name = "LegacyStarChartMode";

                ShipLogStarChartMode = starChartLog.AddComponent<ShipLogStarChartMode>();
                ShipLogLegacyStarChartMode = starChartLogLegacy.AddComponent<ShipLogLegacyStarChartMode>();

                CustomShipLogModesHandler.AddInterstellarMode();
            }

            _starSystemToFactID = new Dictionary<string, string>();
            _factIDToStarSystem = new Dictionary<string, string>();

            _factRequiredToExitViaWarpDrive = string.Empty;

            foreach (NewHorizonsSystem system in _systems)
            {
                if (system.Config.factRequiredForWarp != default && system.UniqueID != "SolarSystem")
                {
                    RegisterFactForSystem(system.Config.factRequiredForWarp, system.UniqueID);
                }

                if (system.UniqueID == Main.Instance.CurrentStarSystem)
                {
                    _factRequiredToExitViaWarpDrive = system.Config.factRequiredToExitViaWarpDrive;
                    _canExitViaWarpDrive = system.Config.canExitViaWarpDrive || !string.IsNullOrEmpty(_factRequiredToExitViaWarpDrive);
                    NHLogger.LogVerbose($"In system {system.UniqueID} can exit via warp drive? {system.Config.canExitViaWarpDrive} {_canExitViaWarpDrive} {_factRequiredToExitViaWarpDrive}");
                }
            }
        }

        /// <summary>
        /// Can the player warp to any system at all right now
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
        /// Can the player ever warp to any system at all
        /// </summary>
        /// <returns></returns>
        public static bool CanEverWarp()
        {
            foreach (var system in _systems)
            {
                if (CanEverWarpToSystem(system.UniqueID))
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

        public static bool HasShipSpawn(string system)
        {
            var config = Main.SystemDict[system];

            var canWarpTo = false;
            if (system.Equals("SolarSystem")) canWarpTo = true;
            else if (system.Equals("EyeOfTheUniverse")) canWarpTo = false;
            else if (config.HasShipSpawn) canWarpTo = true;

            return canWarpTo;
        }

        public static bool CanEnterViaWarpDrive(string system) => system == "SolarSystem" ||
            Main.SystemDict[system].Config.canEnterViaWarpDrive;

        public static bool CanExitViaWarpDrive() => Main.Instance.CurrentStarSystem == "SolarSystem" || (_canExitViaWarpDrive
                && (string.IsNullOrEmpty(_factRequiredToExitViaWarpDrive) || ShipLogHandler.KnowsFact(_factRequiredToExitViaWarpDrive)));

        /// <summary>
        /// Is it actually a valid warp target right now
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public static bool CanWarpToSystem(string system)
        {
            var canWarpTo = CanEverWarpToSystem(system);

            var canExitViaWarpDrive = CanExitViaWarpDrive();

            // Make base system always ignore canExitViaWarpDrive
            if (Main.Instance.CurrentStarSystem == "SolarSystem")
                canExitViaWarpDrive = true;

            NHLogger.LogVerbose(system, canWarpTo, canExitViaWarpDrive, HasUnlockedSystem(system));

            return canWarpTo
                    && canExitViaWarpDrive
                    && system != Main.Instance.CurrentStarSystem
                    && HasUnlockedSystem(system);
        }

        /// <summary>
        /// Is it actually a valid warp target ever
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public static bool CanEverWarpToSystem(string system)
        {
            var hasShipSpawn = HasShipSpawn(system);
            var canEnterViaWarpDrive = CanEnterViaWarpDrive(system);

            NHLogger.LogVerbose(system, hasShipSpawn, canEnterViaWarpDrive);

            return hasShipSpawn && canEnterViaWarpDrive;
        }

        public static void OnRevealFact(string factID)
        {
            if (!string.IsNullOrEmpty(_factRequiredToExitViaWarpDrive) && factID == _factRequiredToExitViaWarpDrive)
            {
                _canExitViaWarpDrive = true;
                if (!Main.HasWarpDriveFunctionality)
                {
                    var flagActuallyAddedAStar = false;
                    // Add all stars that now work
                    foreach (var starSystem in Main.SystemDict.Keys)
                    {
                        if (CanWarpToSystem(starSystem))
                        {
                            ShipLogStarChartMode.AddStarSystem(starSystem);
                            ShipLogLegacyStarChartMode.AddStarSystem(starSystem);
                            flagActuallyAddedAStar = true;
                        }
                    }
                    if (flagActuallyAddedAStar)
                    {
                        Main.Instance.EnableWarpDriveFunctionality();
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
                if (!Main.HasWarpDriveFunctionality && knowsWarpFact)
                {
                    Main.Instance.EnableWarpDriveFunctionality();
                }
                ShipLogStarChartMode.AddStarSystem(systemUnlocked);
                ShipLogLegacyStarChartMode.AddStarSystem(systemUnlocked);
            }
        }

        public static void RegisterFactForSystem(string factID, string system)
        {
            NHLogger.LogVerbose($"Need to know [{factID}] to unlock [{system}]");
            _starSystemToFactID.Add(system, factID);
            _factIDToStarSystem.Add(factID, system);
        }

        public static bool IsWarpDriveLockedOn() => StarChartHandler.CurrentMode.GetTargetStarSystem() != null;

        public static Texture GetSystemCardTexture(string uniqueID)
        {
            Texture texture = null;
            try
            {
                if (uniqueID.Equals("SolarSystem"))
                {
                    texture = ImageUtilities.GetTexture(Main.Instance, "Assets/hearthian system.png");
                }
                else if (uniqueID.Equals("EyeOfTheUniverse"))
                {
                    texture = ImageUtilities.GetTexture(Main.Instance, "Assets/eye symbol.png");
                }
                else
                {
                    var mod = Main.SystemDict[uniqueID].Mod;

                    var path = Path.Combine("systems", uniqueID + ".png");

                    // Else check the old location
                    if (!File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, path)))
                    {
                        path = Path.Combine("planets", uniqueID + ".png");
                    }

                    NHLogger.LogVerbose($"StarChartHandler - Trying to load {path}");
                    texture = ImageUtilities.GetTexture(mod, path);
                }
            }
            catch (System.Exception) { }

            return texture;
        }
    }
}