using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using NewHorizons.External.Configs;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Orbital
{
    static class OrbitlineBuilder
    {
        public static void Make(GameObject body, AstroObject astroobject, bool isMoon, IPlanetConfig config)
        {
            GameObject orbitGO = new GameObject("Orbit");
            orbitGO.transform.parent = body.transform;
            orbitGO.transform.localPosition = Vector3.zero;

            var lineRenderer = orbitGO.AddComponent<LineRenderer>();

            lineRenderer.material = GameObject.Find("OrbitLine_CO").GetComponent<LineRenderer>().material;
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = false;

            var ecc = config.Orbit.Eccentricity;

            var parentGravity = astroobject.GetPrimaryBody()?.GetGravityVolume();

            OrbitLine orbitLine;

            if(config.Orbit.TrackingOrbitLine)
            {
                orbitLine = orbitGO.AddComponent<TrackingOrbitLine>();
            }
            else if (ecc == 0)
            {
                orbitLine = orbitGO.AddComponent<OrbitLine>();
            }
            else if (ecc > 0 && ecc < 1 && (parentGravity != null && parentGravity._falloffType == GravityVolume.FalloffType.inverseSquared))
            {
                // Doesn't work for linear eccentric falloff
                orbitLine = orbitGO.AddComponent<EllipticOrbitLine>();
            }
            else
            {
                orbitLine = orbitGO.AddComponent<TrackingOrbitLine>();
            }

            var color = Color.white;
            if (config.Orbit.Tint != null) color = config.Orbit.Tint.ToColor32();
            else if (config.Star != null) color = config.Star.Tint.ToColor32();
            else if (config.Atmosphere != null && config.Atmosphere.CloudTint != null) color = config.Atmosphere.CloudTint.ToColor32();
            else if (config.Base.BlackHoleSize != 0 || config.Singularity != null) color = new Color(1f, 0.5f, 1f);
            else if (config.Base.WaterSize != 0) color = new Color(0.5f, 0.5f, 1f);
            else if (config.Base.LavaSize != 0) color = new Color(1f, 0.5f, 0.5f);
            else if (config.Atmosphere != null && config.Atmosphere.FogTint != null) color = config.Atmosphere.FogTint.ToColor32();

            var fade = isMoon;
            if (config.Base.IsSatellite)
            {
                if(config.Orbit.Tint != null) color = new Color(0.4082f, 0.516f, 0.4469f, 1f);
                fade = true;
                orbitLine._fadeEndDist = 5000;
                orbitLine._fadeStartDist = 3000;
            }
            
            orbitLine.SetValue("_color", color);

            orbitLine.SetValue("_astroObject", astroobject);
            orbitLine.SetValue("_fade", fade);
            orbitLine.SetValue("_lineWidth", 2f);

            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
                typeof(OrbitLine).GetMethod("InitializeLineRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(orbitLine, new object[] { })   
            );
        }
    }
}
