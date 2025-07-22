#region

using NewHorizons.Components;
using NewHorizons.External.Configs;
using NewHorizons.Handlers;
using UnityEngine;

#endregion

namespace NewHorizons.Builder.General
{
    static class MarkerBuilder
    {
        public static void Make(GameObject body, string name, PlanetConfig config)
        {
            var module = config.MapMarker;
            NHMapMarker mapMarker = body.AddComponent<NHMapMarker>();
            mapMarker._labelID = (UITextType)TranslationHandler.AddUI(config.name, true);

            var markerType = MapMarker.MarkerType.Planet;

            if (config.Orbit.isMoon)
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
            /*
            else if (config.Base.IsSatellite)
            {
                markerType = MapMarker.MarkerType.Probe;
            }
            */

            mapMarker._markerType = markerType;

            mapMarker.minDisplayDistanceOverride = module.minDisplayDistanceOverride;
            mapMarker.maxDisplayDistanceOverride = module.maxDisplayDistanceOverride;
        }
    }
}
