using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Orbital
{
    static class OrbitlineBuilder
    {
        public static void Make(GameObject body, AstroObject astroobject, bool isMoon, OrbitModule orbit)
        {
            GameObject orbitGO = new GameObject("Orbit");
            orbitGO.transform.parent = body.transform;
            orbitGO.transform.localPosition = Vector3.zero;

            var lineRenderer = orbitGO.AddComponent<LineRenderer>();

            lineRenderer.material = GameObject.Find("OrbitLine_CO").GetComponent<LineRenderer>().material;
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = false;

            if(orbit.Eccentricity == 0)
            {
                OrbitLine orbitLine = orbitGO.AddComponent<OrbitLine>();
                orbitLine.SetValue("_astroObject", astroobject);
                orbitLine.SetValue("_fade", isMoon);
                orbitLine.SetValue("_lineWidth", 0.3f);
                typeof(OrbitLine).GetMethod("InitializeLineRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(orbitLine, new object[] { });
            }
            else
            {
                OrbitLine orbitLine = orbitGO.AddComponent<EllipticOrbitLine>();
                orbitLine.SetValue("_astroObject", astroobject);
                orbitLine.SetValue("_fade", isMoon);
                orbitLine.SetValue("_lineWidth", 0.3f);
                typeof(OrbitLine).GetMethod("InitializeLineRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(orbitLine, new object[] { });
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
