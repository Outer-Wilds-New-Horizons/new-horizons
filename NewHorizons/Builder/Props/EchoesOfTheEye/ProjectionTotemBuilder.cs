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
using UnityEngine.InputSystem;

namespace NewHorizons.Builder.Props.EchoesOfTheEye
{
    public static class ProjectionTotemBuilder
    {
        private static GameObject _prefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_3/Interactibles_DreamZone_3/Prefab_IP_DreamObjectProjector_Bridge").InstantiateInactive().Rename("Prefab_ProjectionTotem").DontDestroyOnLoad();
                if (_prefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a grapple totem but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _prefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                    var projector = _prefab.GetComponent<DreamObjectProjector>();
                    projector._projections = new DreamObjectProjection[0];
                }
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, ProjectionTotemInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || sector == null) return null;

            var totemObj = DetailBuilder.Make(planetGO, sector, mod, _prefab, new DetailInfo(info));

            var projector = totemObj.GetComponent<DreamObjectProjector>();

            if (!string.IsNullOrEmpty(info.pathToAlarmTotem))
            {
                var alarmTotemObj = planetGO.transform.Find(info.pathToAlarmTotem);
                if (alarmTotemObj != null)
                {
                    var alarmTotem = alarmTotemObj.GetComponentInChildren<AlarmTotem>();
                    if (alarmTotem != null)
                    {
                        projector._alarmTotem = alarmTotem;
                    }
                }
            }

            if (info.pathsToDreamCandles != null)
            {
                var dreamCandles = new List<DreamCandle>();
                foreach (var pathToDreamCandles in info.pathsToDreamCandles)
                {
                    if (string.IsNullOrEmpty(pathToDreamCandles)) continue;
                    var dreamCandleObj = planetGO.transform.Find(pathToDreamCandles);
                    if (dreamCandleObj != null)
                    {
                        dreamCandles.AddRange(dreamCandleObj.GetComponentsInChildren<DreamCandle>());
                    }
                }
                projector._dreamCandles = dreamCandles.ToArray();
            }

            if (info.pathsToProjectionTotems != null)
            {
                var projectionTotems = new List<DreamObjectProjector>();
                foreach (var pathToProjectionTotems in info.pathsToProjectionTotems)
                {
                    if (string.IsNullOrEmpty(pathToProjectionTotems)) continue;
                    var projectionTotemObj = planetGO.transform.Find(pathToProjectionTotems);
                    if (projectionTotemObj != null)
                    {
                        projectionTotems.AddRange(projectionTotemObj.GetComponentsInChildren<DreamObjectProjector>());
                    }
                }
                projector._extinguishedProjectors = projectionTotems.ToArray();
            }

            if (info.pathsToProjectedObjects != null)
            {
                var projections = new List<DreamObjectProjection>();
                foreach (var pathToProjectedObject in info.pathsToProjectedObjects)
                {
                    if (string.IsNullOrEmpty(pathToProjectedObject)) continue;
                    var projectionObj = planetGO.transform.Find(pathToProjectedObject);
                    if (projectionObj != null)
                    {
                        projectionObj.gameObject.AddComponent<DitheringAnimator>();
                        var projection = projectionObj.gameObject.AddComponent<DreamObjectProjection>();
                        projection._setActive = info.toggleProjectedObjectsActive;
                        projection.Awake();
                        projections.Add(projection);
                    }
                }
                projector._projections = projections.ToArray();
            }

            var sensor = projector._lightSensor as SingleLightSensor;
            sensor._detectFlashlight = true;
            sensor._lightSourceMask |= LightSourceType.FLASHLIGHT;

            projector._lit = info.startLit;
            projector._startLit = info.startLit;
            projector._extinguishOnly = info.extinguishOnly;

            if (info.condition != null)
            {
                var conditionController = projector.gameObject.AddComponent<DreamLightConditionController>();
                conditionController.SetFromInfo(info.condition);
            }

            /*
            Delay.FireOnNextUpdate(() =>
            {
                projector.Start();
            });
            */

            return totemObj;
        }
    }
}
