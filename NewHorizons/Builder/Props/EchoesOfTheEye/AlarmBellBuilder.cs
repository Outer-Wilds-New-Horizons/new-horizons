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
    public static class AlarmBellBuilder
    {
        private static GameObject _prefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = SearchUtilities.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone4/Sector_PrisonDocks/Sector_PrisonInterior/Interactibles_PrisonInterior/Prefab_IP_AlarmBell").InstantiateInactive().Rename("Prefab_AlarmBell").DontDestroyOnLoad();
                if (_prefab == null)
                {
                    NHLogger.LogWarning($"Tried to make an alarm bell but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _prefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                    var lights = SearchUtilities.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone4/Sector_PrisonDocks/Sector_PrisonInterior/Interactibles_PrisonInterior/Effects_IP_AlarmBellLights").Instantiate(Vector3.zero, Quaternion.identity, _prefab.transform).Rename("Effects_AlarmBellLights");
                    var lightController = lights.GetComponent<OWLightController>();
                    var alarmBell = _prefab.GetComponent<AlarmBell>();
                    alarmBell._lightController = lightController;
                    alarmBell._bellTrigger.GetComponent<BoxShape>().enabled = true;
                    alarmBell._oneShotSource.enabled = true;
                }
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, AlarmBellInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || sector == null) return null;

            var bellObj = DetailBuilder.Make(planetGO, ref sector, mod, _prefab, new DetailInfo(info));

            var alarmBell = bellObj.GetComponent<AlarmBell>();

            return bellObj;
        }
    }
}
