using NewHorizons.Components.Props;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.EchoesOfTheEye;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.Props.EchoesOfTheEye
{
    public static class RaftDockBuilder
    {
        private static GameObject _prefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = SearchUtilities.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone1/Structures_Zone1/RaftHouse_ArrivalPatio_Zone1/Interactables_RaftHouse_ArrivalPatio_Zone1/Prefab_IP_RaftDock").InstantiateInactive().Rename("Prefab_RaftDock").DontDestroyOnLoad();
                if (_prefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a raft dock but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _prefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                    var raftDock = _prefab.GetComponent<RaftDock>();
                    raftDock._startRaft = null;
                    raftDock._floodSensor = null;
                    foreach (var floodToggle in _prefab.GetComponents<FloodToggle>())
                    {
                        Component.DestroyImmediate(floodToggle);
                    }
                    Object.DestroyImmediate(_prefab.FindChild("FloodSensor"));
                    Object.DestroyImmediate(_prefab.FindChild("FloodSensor_RaftHouseArrivalPatio_NoDelay"));
                }
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, RaftDockInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || sector == null) return null;

            var dockObject = DetailBuilder.Make(planetGO, ref sector, mod, _prefab, new DetailInfo(info));

            //var raftDock = dockObject.GetComponent<RaftDock>();

            return dockObject;
        }
    }
}
