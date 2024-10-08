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
    public static class AlarmTotemBuilder
    {
        private static GameObject _prefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_Underground/IslandsRoot/IslandPivot_C/Island_C/Interactibles_Island_C/Prefab_IP_AlarmTotem").InstantiateInactive().Rename("Prefab_AlarmTotem").DontDestroyOnLoad();
                if (_prefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a grapple totem but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _prefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                    var alarmTotem = _prefab.GetComponent<AlarmTotem>();
                    alarmTotem._sector = null;
                }
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, AlarmTotemInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || sector == null) return null;

            var totemObj = DetailBuilder.Make(planetGO, sector, mod, _prefab, new DetailInfo(info));

            var alarmTotem = totemObj.GetComponent<AlarmTotem>();
            alarmTotem._sightAngle = info.sightAngle;
            alarmTotem._sightDistance = info.sightDistance;

            return totemObj;
        }
    }
}
