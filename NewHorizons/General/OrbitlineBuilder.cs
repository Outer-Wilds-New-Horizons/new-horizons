using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class OrbitlineBuilder
    {
        public static void Make(GameObject body, AstroObject astroobject, bool isMoon, OrbitModule orbit)
        {
            GameObject orbitGO = new GameObject("Orbit");
            orbitGO.transform.parent = body.transform;

            var LR = orbitGO.AddComponent<LineRenderer>();

            var thLR = GameObject.Find("OrbitLine_CO").GetComponent<LineRenderer>();

            LR.material = thLR.material;
            LR.useWorldSpace = false;
            LR.loop = false;

            OrbitLine ol = orbit.Eccentricity != 0 ? orbitGO.AddComponent<EllipticOrbitLine>() : orbitGO.AddComponent<OrbitLine>();
            ol.SetValue("_astroObject", astroobject);
            ol.SetValue("_fade", isMoon);
            ol.SetValue("_lineWidth", 0.5f);
            //ol.SetOrbitalParameters(orbit.Eccentricity, orbit.SemiMajorAxis, orbit.Inclination, orbit.LongitudeOfAscendingNode, orbit.ArgumentOfPeriapsis, orbit.TrueAnomaly);
            typeof(OrbitLine).GetMethod("InitializeLineRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(ol, new object[] { });
        }
    }
}
