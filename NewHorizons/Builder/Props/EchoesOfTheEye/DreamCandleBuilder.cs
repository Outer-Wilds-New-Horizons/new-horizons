using NewHorizons.Components.EOTE;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.EchoesOfTheEye;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Props.EchoesOfTheEye
{
    public static class DreamCandleBuilder
    {
        private static Dictionary<DreamCandleType, GameObject> _prefabs = new();

        internal static void InitPrefabs()
        {
            InitPrefab(DreamCandleType.Ground, "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Interactibles_DreamZone_1/DreamHouseIsland/Prefab_IP_DreamCandle");
            InitPrefab(DreamCandleType.GroundSmall, "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2/Structure_DreamZone_2/City/StartingAreaLanterns/Prefab_IP_DreamCandle_Ground_Small");
            InitPrefab(DreamCandleType.GroundLarge, "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Interactibles_DreamZone_1/DreamHouseIsland/Prefab_IP_DreamCandle_Ground_Large");
            InitPrefab(DreamCandleType.GroundSingle, "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Sector_PartyHouse/Interactables_PartyHouse/Prefab_IP_DreamCandle_Ground_Single");
            InitPrefab(DreamCandleType.Wall, "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2/Structure_DreamZone_2/City/ParkLanterns/Prefab_IP_DreamCandle_Wall");
            InitPrefab(DreamCandleType.WallLargeFlame, "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2/Structure_DreamZone_2/City/FalseKnightHouse/CandleDoor/FirstDoorLanterns/Prefab_IP_DreamCandle_LargeFlame_Wall");
            InitPrefab(DreamCandleType.WallBigWick, "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2/Structure_DreamZone_2/DreamFireHouse_2/Interactibles_DreamFireHouse_2/Pivot_SlideReelRoom/CandleController/CandlePivot_0/Prefab_IP_DreamCandle_BigWick_Wall");
            InitPrefab(DreamCandleType.Standing, "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2/Structure_DreamZone_2/City/ElevatorHouse/CandleDoor/DoorTutorial/Prefab_IP_DreamCandle_LargeFlame_Standing");
            InitPrefab(DreamCandleType.Pile, "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_3/Sector_Hotel/Gallery/Interactibles_Gallery/DreamCandles/Prefab_IP_DreamCandle_Pile");
        }

        private static void InitPrefab(DreamCandleType type, string path)
        {
            var prefab = _prefabs.ContainsKey(type) ? _prefabs[type] : null;
            if (prefab == null)
            {
                prefab = SearchUtilities.Find(path).InstantiateInactive().Rename($"Prefab_DreamCandle_{type}").DontDestroyOnLoad();
                if (prefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a dream candle but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    prefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                    var sensor = prefab.GetComponentInChildren<SingleLightSensor>();
                    sensor._sector = null;
                }
                _prefabs.Add(type, prefab);
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, DreamCandleInfo info, IModBehaviour mod)
        {
            InitPrefabs();

            var prefab = _prefabs.ContainsKey(info.type) ? _prefabs[info.type] : null;

            if (prefab == null || sector == null) return null;

            var candleObj = DetailBuilder.Make(planetGO, sector, mod, prefab, new DetailInfo(info));

            var dreamCandle = candleObj.GetComponent<DreamCandle>();

            var sensor = candleObj.GetComponentInChildren<SingleLightSensor>();
            sensor._detectFlashlight = true;
            sensor._lightSourceMask |= LightSourceType.FLASHLIGHT;

            dreamCandle._startLit = info.startLit;
            dreamCandle.SetLit(info.startLit, false, true);

            if (info.condition != null)
            {
                var conditionController = dreamCandle.gameObject.AddComponent<DreamLightConditionController>();
                conditionController.SetFromInfo(info.condition);
            }

            return candleObj;
        }
    }
}
