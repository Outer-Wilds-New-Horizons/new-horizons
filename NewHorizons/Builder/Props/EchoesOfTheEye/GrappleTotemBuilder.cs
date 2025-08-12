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
    public static class GrappleTotemBuilder
    {
        private static GameObject _prefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_4/Interactibles_DreamZone_4_Upper/Prefab_IP_GrappleTotem").InstantiateInactive().Rename("Prefab_GrappleTotem").DontDestroyOnLoad();
                if (_prefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a grapple totem but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _prefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                    var zoomPoint = _prefab.GetComponentInChildren<LanternZoomPoint>();
                    zoomPoint._sector = null;
                    var sensor = _prefab.GetComponentInChildren<SingleLightSensor>();
                    sensor._sector = null;
                }
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, GrappleTotemInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || sector == null) return null;

            var totemObj = DetailBuilder.Make(planetGO, ref sector, mod, _prefab, new DetailInfo(info));

            var zoomPoint = totemObj.GetComponentInChildren<LanternZoomPoint>();
            zoomPoint._minActivationDistance = info.minDistance;
            zoomPoint._arrivalDistance = info.arrivalDistance;

            var sensor = totemObj.GetComponentInChildren<SingleLightSensor>();
            sensor._detectionAngle = info.maxAngle;
            sensor._maxDistance = info.maxDistance;

            sensor._detectFlashlight = true;
            sensor._lightSourceMask |= LightSourceType.FLASHLIGHT;

            return totemObj;
        }
    }
}
