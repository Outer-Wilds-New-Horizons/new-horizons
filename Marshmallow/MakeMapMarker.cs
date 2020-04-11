using OWML.ModHelper.Events;
using System.Reflection;
using UnityEngine;

namespace Marshmallow.General
{
    static class MakeMapMarker
    {
        public static void Make(GameObject body)
        {
            var MM = body.AddComponent<MapMarker>();
            MM.SetValue("_labelID", UITextType.YouAreDeadMessage);
            MM.SetValue("_markerType", MM.GetType().GetNestedType("MarkerType", BindingFlags.NonPublic).GetField("Planet").GetValue(MM));
        }
    }
}
