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
            GameObject entryLocationGameObject = new GameObject(!string.IsNullOrEmpty(info.rename) ? info.rename : ("Entry Location (" + info.id + ")"));
            entryLocationGameObject.SetActive(false);
            entryLocationGameObject.transform.parent = sector?.transform ?? go.transform;

            if (!string.IsNullOrEmpty(info.parentPath))
            {
                var newParent = go.transform.Find(info.parentPath);
                if (newParent != null)
                {
                    entryLocationGameObject.transform.parent = newParent;
                }
                else
                {
                    Logger.LogWarning($"Cannot find parent object at path: {go.name}/{info.parentPath}");
                }
            }

            var pos = (Vector3)(info.position ?? Vector3.zero);
            if (info.isRelativeToParent)
                entryLocationGameObject.transform.localPosition = pos;
            else
                entryLocationGameObject.transform.position = go.transform.TransformPoint(pos);

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
