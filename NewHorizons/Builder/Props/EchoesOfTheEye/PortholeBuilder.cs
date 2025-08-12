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
    public static class PortholeBuilder
    {
        private static GameObject _mainPrefab;
        private static GameObject _simPrefab;

        internal static void InitPrefabs()
        {
            if (_mainPrefab == null)
            {
                _mainPrefab = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_4/Interactibles_DreamZone_4_Upper/Props_IP_Peephole_Prison").InstantiateInactive().Rename("Prefab_Porthole").DontDestroyOnLoad();
                if (_mainPrefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a grapple totem but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _mainPrefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                    var peephole = _mainPrefab.GetComponentInChildren<Peephole>();
                    peephole._factIDs = new string[0];
                    peephole._viewingSector = null;
                }
            }
            if (_simPrefab == null)
            {
                _simPrefab = SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_4/Simulation_DreamZone_4/Geo_DreamZone_4_Upper/Effects_IP_SIM_Porthole").InstantiateInactive().Rename("Prefab_SIM_Porthole").DontDestroyOnLoad();
                if (_simPrefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a grapple totem but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _simPrefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                }
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, PortholeInfo info, IModBehaviour mod)
        {
            InitPrefabs();

            if (_mainPrefab == null || _simPrefab == null || sector == null) return null;

            var portholeSector = sector;
            var portholeObj = DetailBuilder.Make(planetGO, ref portholeSector, mod, _mainPrefab, new DetailInfo(info));
            portholeObj.name = "Prefab_Porthole";

            var simSector = sector;
            var simObj = DetailBuilder.Make(planetGO, ref simSector, mod, _simPrefab, new DetailInfo(info));
            simObj.transform.parent = portholeObj.transform;

            var parentObj = GeneralPropBuilder.MakeNew("Porthole", planetGO, ref sector, info);
            parentObj.SetActive(true);
            portholeObj.transform.SetParent(parentObj.transform, true);
            portholeObj.transform.localPosition = new Vector3(0f, -4f, 8f);
            portholeObj.transform.localEulerAngles = new Vector3(0f, 315f, 0f);

            var peephole = portholeObj.GetComponentInChildren<Peephole>();
            if (info.revealFacts != null)
            {
                peephole._factIDs = info.revealFacts;
            }

            peephole._peepholeCamera.farClipPlane = 4000f;
            peephole._peepholeCamera.fieldOfView = info.fieldOfView;

            // Reposition the peephole camera later, after all planets are built, in case the target point is on a different astro body.
            Delay.FireInNUpdates(() =>
            {
                var viewingSector = sector;
                if (string.IsNullOrEmpty(info.target.sectorPath))
                {
                    info.target.sectorPath = "auto";
                }
                var cameraObj = GeneralPropBuilder.MakeFromExisting(peephole._peepholeCamera.gameObject, planetGO, ref viewingSector, info.target);
                cameraObj.transform.Rotate(Vector3.up, 180f, Space.Self);
                cameraObj.transform.position += cameraObj.transform.up;
                peephole._viewingSector = viewingSector;
            }, 2);

            return portholeObj;
        }
    }
}
