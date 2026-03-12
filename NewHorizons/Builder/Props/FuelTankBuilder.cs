using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.EchoesOfTheEye;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Utils;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class FuelTankBuilder
    {
        private static GameObject _prefab;
        private static GameObject _dlcPrefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = SearchUtilities.Find("Moon_Body/Sector_THM/Interactables_THM/Prefab_HEA_FuelTank").InstantiateInactive().Rename("Prefab_FuelTank").DontDestroyOnLoad();
            }
        }

        internal static void InitDLCPrefab()
        {
            if (_dlcPrefab == null)
            {
                _dlcPrefab = SearchUtilities.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone1/Sector_SlideBurningRoom_Zone1/Interactables_SlideBurningRoom_Zone1/TorchRoot/Prefab_IP_FuelTorch").InstantiateInactive().Rename("Prefab_FuelTorch").DontDestroyOnLoad();
                if (_dlcPrefab == null)
                {
                    NHLogger.LogWarning($"Tried to make a DLC fuel tank but couldn't. Do you have the DLC installed?");
                    return;
                }
                else
                {
                    _dlcPrefab.AddComponent<DestroyOnDLC>()._destroyOnDLCNotOwned = true;
                }
            }
        }

        private static GameObject GetPrefab(FuelTankInfo.FuelTankType type)
        {
            switch (type)
            {
                case FuelTankInfo.FuelTankType.HearthianTank:
                    return _prefab;
                case FuelTankInfo.FuelTankType.DLCTorch:
                    InitDLCPrefab();
                    return _dlcPrefab;
                default:
                    return null;
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, FuelTankInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (sector == null) return null;

            var prefab = GetPrefab(info.type);
            if (prefab == null)
            {
                NHLogger.LogWarning("Tried to make a fuel tank but couldn't find a matching prefab.");
                return null;
            }

            var fuelTank = DetailBuilder.Make(planetGO, ref sector, mod, prefab, new DetailInfo(info));

            //var playerRecoveryPoint = fuelTank.GetComponentInChildren<PlayerRecoveryPoint>();

            return fuelTank;
        }
    }
}
