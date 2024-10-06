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
    public static class DreamCampfireBuilder
    {
        private static GameObject _prefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = SearchUtilities.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone4/Sector_PrisonDocks/Sector_PrisonInterior/Interactibles_PrisonInterior/Prefab_IP_DreamCampfire").InstantiateInactive().Rename("Prefab_DreamCampfire").DontDestroyOnLoad();
                if (_prefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a dream campfire but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _prefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                    var campfire = _prefab.GetComponentInChildren<DreamCampfire>();
                    campfire._dreamArrivalLocation = DreamArrivalPoint.Location.Undefined;
                    campfire._sector = null;
                    campfire._entrywayVolumes = [];
                }
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, DreamCampfireInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || sector == null) return null;

            var campfireObj = DetailBuilder.Make(planetGO, sector, mod, _prefab, new DetailInfo(info));

            var campfire = campfireObj.GetComponentInChildren<DreamCampfire>();
            campfire._dreamArrivalLocation = DreamHandler.GetDreamArrivalLocation(info.id);

            // The streaming groups on DreamCampfires get set on Start() so we wait until after to change it again
            Delay.FireInNUpdates(() => {
                var streaming = campfireObj.GetComponentInChildren<DreamCampfireStreaming>();
                if (streaming != null)
                {
                    var targetArrivalPoint = Locator.GetDreamArrivalPoint(campfire._dreamArrivalLocation);
                    if (targetArrivalPoint != null)
                    {
                        var streamingGroup = targetArrivalPoint.transform.root.GetComponentInChildren<StreamingGroup>();
                        if (streamingGroup)
                        {
                            streaming._streamingGroup = streamingGroup;
                        }
                    }
                }
            }, 2);

            Locator.RegisterDreamCampfire(campfire, campfire._dreamArrivalLocation);

            return campfireObj;
        }
    }
}
