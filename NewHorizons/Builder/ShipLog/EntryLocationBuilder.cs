using NewHorizons.Builder.Props;
using NewHorizons.External.Modules;
using OWML.Common;
using System.Collections.Generic;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.ShipLog
{
    public static class EntryLocationBuilder
    {
        private static readonly List<ShipLogEntryLocation> _locationsToInitialize = new List<ShipLogEntryLocation>();
        public static void Make(GameObject go, Sector sector, PropModule.EntryLocationInfo info, IModBehaviour mod)
        {
            GameObject entryLocationGameObject = GeneralPropBuilder.MakeNew("Entry Location (" + info.id + ")", sector?.transform ?? go.transform, info);

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
