#region

using System.Collections.Generic;
using NewHorizons.External.Modules;
using OWML.Common;
using UnityEngine;

#endregion

namespace NewHorizons.Builder.ShipLog
{
    public static class EntryLocationBuilder
    {
        private static readonly List<ShipLogEntryLocation> _locationsToInitialize = new List<ShipLogEntryLocation>();

        public static void Make(GameObject go, Sector sector, PropModule.EntryLocationInfo info, IModBehaviour mod)
        {
            var entryLocationGameObject = new GameObject("Entry Location (" + info.id + ")");
            entryLocationGameObject.SetActive(false);
            entryLocationGameObject.transform.parent = sector?.transform ?? go.transform;
            entryLocationGameObject.transform.position = go.transform.TransformPoint(info.position ?? Vector3.zero);
            var newLocation = entryLocationGameObject.AddComponent<ShipLogEntryLocation>();
            newLocation._entryID = info.id;
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