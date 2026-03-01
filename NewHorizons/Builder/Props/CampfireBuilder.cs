using NewHorizons.External.Modules.Props;
using NewHorizons.Utility;
using OWML.Common;
using OWML.Utils;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class CampfireBuilder
    {
        private static GameObject _prefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                _prefab = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Sector_RadioTower/RadioTower_DLC/Interactibles_RadioTower/Hornfels'GrovePivot/Prefab_HEA_Campfire").InstantiateInactive().Rename("Prefab_Campfire").DontDestroyOnLoad();
                var campfire = _prefab.GetComponentInChildren<Campfire>();
                campfire._sector = null;
                campfire._playerInSector = false;
            }
        }

        public static void SetupCampfire(Campfire campfire, CampfireInfo info)
        {
            if (campfire == null) return;
            campfire._canSleepHere = info.canSleepHere;
            campfire._lookUpWhileSleeping = info.lookUpWhileSleeping;
            var initialState = EnumUtils.Parse<Campfire.State>(info.initialState.ToString());
            campfire.SetInitialState(initialState);
            campfire.SetState(initialState, true);
        }

        public static GameObject Make(GameObject planetGO, Sector sector, CampfireInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || sector == null) return null;

            var campfireObj = DetailBuilder.Make(planetGO, ref sector, mod, _prefab, new DetailInfo(info));

            var campfire = campfireObj.GetComponentInChildren<Campfire>();
            SetupCampfire(campfire, info);

            return campfireObj;
        }
    }
}
