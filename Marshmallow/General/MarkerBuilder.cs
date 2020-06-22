using Marshmallow.External;
using OWML.ModHelper.Events;
using System.Reflection;
using UnityEngine;
using Logger = Marshmallow.Utility.Logger;

namespace Marshmallow.General
{
    static class MarkerBuilder
    {
        public static void Make(GameObject body, IPlanetConfig config)
        {
            MapMarker MM = body.AddComponent<MapMarker>();
            MM.SetValue("_labelID", (UITextType)Utility.AddToUITable.Add(config.Name));

            if (config.IsMoon)
            {
                MM.SetValue("_markerType", MM.GetType().GetNestedType("MarkerType", BindingFlags.NonPublic).GetField("Moon").GetValue(MM));
            }
            else
            {
                MM.SetValue("_markerType", MM.GetType().GetNestedType("MarkerType", BindingFlags.NonPublic).GetField("Planet").GetValue(MM));
            }
            Logger.Log("Finished building map marker", Logger.LogType.Log);
        }
    }
}
