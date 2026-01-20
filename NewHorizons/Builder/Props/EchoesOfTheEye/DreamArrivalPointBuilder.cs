using NewHorizons.Components.Props;
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
    public static class DreamArrivalPointBuilder
    {
        private static GameObject _prefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/DreamFireHouse_1/Interactibles_DreamFireHouse_1/Prefab_IP_DreamArrivalPoint_Zone1").InstantiateInactive().Rename("Prefab_DreamArrivalPoint").DontDestroyOnLoad();
                if (_prefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a dream arrival point but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _prefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                    var dreamArrivalPoint = _prefab.GetComponent<DreamArrivalPoint>();
                    dreamArrivalPoint._location = DreamArrivalPoint.Location.Undefined;
                    dreamArrivalPoint._sector = null;
                    dreamArrivalPoint._entrywayVolumes = new OWTriggerVolume[0];
                    dreamArrivalPoint._raftSpawn = null;
                    dreamArrivalPoint._connectedDreamCampfire = null;
                    dreamArrivalPoint._campfire._sector = null;
                }
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, DreamArrivalPointInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || sector == null) return null;

            var arrivalPointObj = DetailBuilder.Make(planetGO, ref sector, mod, _prefab, new DetailInfo(info));

            StreamingHandler.SetUpStreaming(arrivalPointObj, sector);

            DreamArrivalPoint arrivalPoint = arrivalPointObj.GetComponent<DreamArrivalPoint>();
            arrivalPoint._sector = arrivalPoint.GetComponentInParent<Sector>();
            arrivalPoint._location = DreamHandler.GetDreamArrivalLocation(info.id);
            Locator.RegisterDreamArrivalPoint(arrivalPoint, arrivalPoint._location);

            return arrivalPointObj;
        }
    }
}
