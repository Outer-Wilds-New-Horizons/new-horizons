using NewHorizons.Components;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using UnityEngine.UI;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.ShipLog
{
    public static class EntryLocationBuilder
    {
        private static readonly List<ShipLogEntryLocation> _locationsToInitialize = new List<ShipLogEntryLocation>();
        public static void Make(GameObject go, Sector sector, PropModule.EntryLocationInfo info, IModBehaviour mod)
        {
            GameObject entryLocationGameObject = new GameObject("Entry Location (" + info.id + ")");
            entryLocationGameObject.SetActive(false);
            entryLocationGameObject.transform.parent = sector?.transform ?? go.transform;
            entryLocationGameObject.transform.localPosition = info.position ?? Vector3.zero;
            ShipLogEntryLocation newLocation = entryLocationGameObject.AddComponent<ShipLogEntryLocation>();
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
