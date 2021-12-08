using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class OrbitlineBuilder
    {
        public static void Make(GameObject body, AstroObject astroobject, bool isMoon)
        {
            GameObject orbit = new GameObject("Orbit");
            orbit.transform.parent = body.transform;

            var LR = orbit.AddComponent<LineRenderer>();

            var thLR = GameObject.Find("OrbitLine_TH").GetComponent<LineRenderer>();

            LR.material = thLR.material;
            LR.useWorldSpace = false;
            LR.loop = false;

            Logger.Log("AO primary body is " + astroobject.GetPrimaryBody().name, Logger.LogType.Log);

            var ol = orbit.AddComponent<OrbitLine>();
            ol.SetValue("_astroObject", astroobject);
            ol.SetValue("_fade", isMoon);
            ol.SetValue("_lineWidth", 0.5f);
            typeof(OrbitLine).GetMethod("InitializeLineRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(ol, new object[] { });

            Logger.Log("Finished building orbit line", Logger.LogType.Log);
        }
    }
}
