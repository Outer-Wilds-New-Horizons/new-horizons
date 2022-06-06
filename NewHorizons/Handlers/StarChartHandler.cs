using NewHorizons.Components;
using NewHorizons.Utility;
using System.Collections.Generic;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
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

            var starChartLog = new GameObject("StarChartMode");
            starChartLog.SetActive(false);
            starChartLog.transform.parent = shipLogRoot.transform;
            starChartLog.transform.localScale = Vector3.one * 1f;
            starChartLog.transform.localPosition = Vector3.zero;
            starChartLog.transform.localRotation = Quaternion.Euler(0, 0, 0);

            ShipLogStarChartMode = starChartLog.AddComponent<ShipLogStarChartMode>();

            var reticleImage = GameObject.Instantiate(SearchUtilities.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ReticleImage (1)/"), starChartLog.transform);

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

            var centerPromptList = shipLogRoot.transform.Find("ScreenPromptListScaleRoot/ScreenPromptList_Center")?.GetComponent<ScreenPromptList>();
            var upperRightPromptList = shipLogRoot.transform.Find("ScreenPromptListScaleRoot/ScreenPromptList_UpperRight")?.GetComponent<ScreenPromptList>();
            var oneShotSource = SearchUtilities.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/OneShotAudio_ShipLog")?.GetComponent<OWAudioSource>();

            _starSystemToFactID = new Dictionary<string, string>();
            _factIDToStarSystem = new Dictionary<string, string>();

            foreach (NewHorizonsSystem system in _systems)
            {
                if (system.Config.factRequiredForWarp != default)
                {
                    RegisterFactForSystem(system.Config.factRequiredForWarp, system.UniqueID);
                }
            }

            ShipLogStarChartMode.Initialize(
                centerPromptList,
                upperRightPromptList,
                oneShotSource);
        }

        public static bool CanWarp()
        {
            foreach (var system in _systems)
            {
                if (system.Config.canEnterViaWarpDrive && system.Spawn?.shipSpawnPoint != null && HasUnlockedSystem(system.UniqueID))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasUnlockedSystem(string system)
        {
            if (_starSystemToFactID == null || _starSystemToFactID.Count == 0)
                return true;

            // If we can't get a fact for the system, then its unlocked
            if (!_starSystemToFactID.TryGetValue(system, out var factID))
                return true;

            // If we got a fact but now can't find it elsewhere, its not unlocked
            if (!GameObject.FindObjectOfType<ShipLogManager>()._factDict.TryGetValue(factID, out var fact))
                return false;

            // It's unlocked if revealed
            return fact.IsRevealed();
        }

        public static void OnRevealFact(string factID)
        {
            if (_factIDToStarSystem.TryGetValue(factID, out var systemUnlocked))
            {
                Logger.Log($"Just learned [{factID}] and unlocked [{systemUnlocked}]");
                if (!Main.HasWarpDrive) Main.Instance.EnableWarpDrive();
                ShipLogStarChartMode.AddSystemCard(systemUnlocked);
            }
        }

        public static void RegisterFactForSystem(string factID, string system)
        {
            Logger.Log($"Need to know [{factID}] to unlock [{system}]");
            _starSystemToFactID.Add(system, factID);
            _factIDToStarSystem.Add(factID, system);
        }
    }
}