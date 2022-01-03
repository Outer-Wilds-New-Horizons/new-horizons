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

            var markerType = MapMarker.MarkerType.Planet;

            if (config.Orbit.IsMoon)
            {
                markerType = MapMarker.MarkerType.Moon;
            }
            else if (config.Star != null)
            {
                markerType = MapMarker.MarkerType.Sun;
            }
            else if (config.FocalPoint != null)
            {
                markerType = MapMarker.MarkerType.HourglassTwins;
            }
            else if(config.Base.IsSatellite)
            {
                markerType = MapMarker.MarkerType.Probe;
            }

            mapMarker._markerType = markerType;
        }
    }
}
