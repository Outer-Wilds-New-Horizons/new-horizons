using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.EchoesOfTheEye;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Utils;
using UnityEngine;
using static NewHorizons.Utility.Files.AssetBundleUtilities;

namespace NewHorizons.Builder.Props
{
    public static class FuelTankBuilder
    {
        private static GameObject _hearthianPrefab;
        private static GameObject _nomaiPrefab;
        private static GameObject _nomaiPreCrashPrefab;
        private static GameObject _dlcPrefab;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_hearthianPrefab == null)
            {
                _hearthianPrefab = SearchUtilities.Find("Moon_Body/Sector_THM/Interactables_THM/Prefab_HEA_FuelTank").InstantiateInactive().Rename("Prefab_HEA_FuelTank").DontDestroyOnLoad();
            }

            if (_isInit) return;

            _isInit = true;

            _nomaiPrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_NOM_FuelTank");
            _nomaiPreCrashPrefab = NHPrivateAssetBundle.LoadAsset<GameObject>("Prefab_NOM_Vessel_FuelTank");
            AssetBundleUtilities.ReplaceShaders(_nomaiPrefab);
            AssetBundleUtilities.ReplaceShaders(_nomaiPreCrashPrefab);
        }

        internal static void InitDLCPrefab()
        {
            if (_dlcPrefab == null)
            {
                _dlcPrefab = SearchUtilities.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone4/Sector_BlightedShore/Sector_JammingControlRoom_Zone4/Interactables_JammingControlRoom_Zone4/Prefab_IP_FuelTorch").InstantiateInactive().Rename("Prefab_IP_FuelTorch").DontDestroyOnLoad();
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
                    return _hearthianPrefab;
                case FuelTankInfo.FuelTankType.NomaiTank:
                    return _nomaiPrefab;
                case FuelTankInfo.FuelTankType.PreCrashNomaiTank:
                    return _nomaiPreCrashPrefab;
                case FuelTankInfo.FuelTankType.DLCTorch:
                    InitDLCPrefab();
                    return _dlcPrefab;
                default:
                    return null;
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, FuelTankInfo info, IModBehaviour mod)
        {
            InitPrefabs();

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
