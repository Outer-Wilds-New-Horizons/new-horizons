using NewHorizons.External;
using OWML.Utils;
using System.Reflection;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    static class MarkerBuilder
    {
        public static void Make(GameObject body, string name, IPlanetConfig config)
        {
            MapMarker mapMarker = body.AddComponent<MapMarker>();
            mapMarker.SetValue("_labelID", (UITextType)Utility.AddToUITable.Add(name.ToUpper()));

            var markerType = "Planet";

            if (config.Orbit.IsMoon)
            {
                markerType = "Moon";
            }
            else if (config.Star != null)
            {
                markerType = "Sun";
            }
            else if (config.FocalPoint != null)
            {
                markerType = "HourglassTwins";
            }

            mapMarker.SetValue("_markerType", mapMarker.GetType().GetNestedType("MarkerType", BindingFlags.NonPublic).GetField(markerType).GetValue(mapMarker));
        }
    }
}
