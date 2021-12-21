using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
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
            orbitGO.transform.localPosition = Vector3.zero;

            var LR = orbitGO.AddComponent<LineRenderer>();

            var thLR = GameObject.Find("OrbitLine_CO").GetComponent<LineRenderer>();

            LR.material = thLR.material;
            LR.useWorldSpace = false;
            LR.loop = false;

            if(orbit.Eccentricity == 0)
            {
                OrbitLine ol = orbitGO.AddComponent<OrbitLine>();
                ol.SetValue("_astroObject", astroobject);
                ol.SetValue("_fade", isMoon);
                ol.SetValue("_lineWidth", 0.5f);
                typeof(OrbitLine).GetMethod("InitializeLineRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(ol, new object[] { });
            }
            else
            {
                OrbitLine ol = orbitGO.AddComponent<EllipticOrbitLine>();
                ol.SetValue("_astroObject", astroobject);
                ol.SetValue("_fade", isMoon);
                ol.SetValue("_lineWidth", 0.5f);
                typeof(OrbitLine).GetMethod("InitializeLineRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(ol, new object[] { });
            }

            /*
            ParameterizedOrbitLine orbitLine = orbitGO.AddComponent<ParameterizedOrbitLine>();
            
            orbitLine.SetValue("_astroObject", astroobject);
            orbitLine.SetValue("_fade", isMoon);
            orbitLine.SetValue("_lineWidth", 0.5f);

            orbitLine.SetOrbitalParameters(
                orbit.Eccentricity,
                orbit.SemiMajorAxis, 
                orbit.Inclination, 
                orbit.LongitudeOfAscendingNode, 
                orbit.ArgumentOfPeriapsis, 
                orbit.TrueAnomaly);

            typeof(OrbitLine).GetMethod("InitializeLineRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(orbitLine, new object[] { });
            */
        }
    }
}
