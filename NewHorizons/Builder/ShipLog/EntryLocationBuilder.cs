using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Props;
using OWML.Common;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Builder.ShipLog
{
    public static class EntryLocationBuilder
    {
        private static readonly List<ShipLogEntryLocation> _locationsToInitialize = new List<ShipLogEntryLocation>();
        public static void Make(GameObject go, Sector sector, EntryLocationInfo info, IModBehaviour mod)
        {
            GameObject entryLocationGameObject = GeneralPropBuilder.MakeNew("Entry Location (" + info.id + ")", go, ref sector, info);

            ShipLogEntryLocation newLocation = entryLocationGameObject.AddComponent<ShipLogEntryLocation>();
            newLocation._entryID = info.id;
            newLocation._outerFogWarpVolume = go.GetComponentInChildren<OuterFogWarpVolume>();
            newLocation._isWithinCloakField = info.cloaked;
            _locationsToInitialize.Add(newLocation);
            entryLocationGameObject.SetActive(true);
        }

        public static void InitializeLocations()
        {
            _locationsToInitialize.ForEach(l => l.InitEntry());
            _locationsToInitialize.Clear();
        }
    }
}
