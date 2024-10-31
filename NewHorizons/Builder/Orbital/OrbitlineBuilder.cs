using NewHorizons.Components.Orbital;
using NewHorizons.External.Configs;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using UnityEngine;
namespace NewHorizons.Builder.Orbital
{
    public static class OrbitlineBuilder
    {
        private static Material _dottedLineMaterial;
        private static Material _lineMaterial;

        public static GameObject Make(GameObject planetGO, bool isMoon, PlanetConfig config)
        {
            var orbitGO = new GameObject("Orbit");
            orbitGO.transform.parent = planetGO.transform;
            orbitGO.transform.localPosition = Vector3.zero;

            Delay.FireOnNextUpdate(() => PostMake(orbitGO, planetGO, isMoon, config));
            return orbitGO;
        }

        private static void PostMake(GameObject orbitGO, GameObject planetGO, bool isMoon, PlanetConfig config)
        {
            if (_dottedLineMaterial == null) _dottedLineMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Effects_SPA_OrbitLine_Dotted_mat");
            if (_lineMaterial == null) _lineMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Effects_SPA_OrbitLine_mat");

            // Might've been otherwise destroyed when updating
            if (orbitGO == null) return;

            var astroObject = planetGO.GetComponent<NHAstroObject>();

            var lineRenderer = orbitGO.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(config.Orbit.dottedOrbitLine ? _dottedLineMaterial : _lineMaterial);
            lineRenderer.textureMode = config.Orbit.dottedOrbitLine ? LineTextureMode.RepeatPerSegment : LineTextureMode.Stretch;

            var width = config.Orbit.dottedOrbitLine ? 100 : 50;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = false;

            var numVerts = config.Orbit.dottedOrbitLine ? 128 : 256;
            lineRenderer.positionCount = numVerts;

            var ecc = config.Orbit.eccentricity;

            var parentGravity = astroObject.GetPrimaryBody()?.GetGravityVolume();

            OrbitLine orbitLine;
            if (config.Orbit.trackingOrbitLine || (new Gravity(parentGravity).Power == 1 && ecc != 0))
            {
                orbitLine = orbitGO.AddComponent<TrackingOrbitLine>();
            }
            else
            {
                orbitLine = orbitGO.AddComponent<NHOrbitLine>();
                (orbitLine as NHOrbitLine).SetFromParameters(astroObject);
            }

            var color = Color.white;
            if (config.Orbit.tint != null) color = config.Orbit.tint.ToColor();
            else if (config.Star?.tint != null) color = config.Star.tint.ToColor();
            else if (config.Atmosphere?.clouds?.tint != null) color = config.Atmosphere.clouds.tint.ToColor();
            else if (config.Water != null) color = new Color(0.5f, 0.5f, 1f);
            else if (config.Lava != null) color = new Color(1f, 0.5f, 0.5f);
            else if (config.Atmosphere != null && config.Atmosphere.fogTint != null) color = config.Atmosphere.fogTint.ToColor();

            var fade = isMoon;

            if (config.Orbit.orbitLineFadeStartDistance >= 0)
            {
                fade = true;
                orbitLine._fadeStartDist = config.Orbit.orbitLineFadeStartDistance;
            }

            if (config.Orbit.orbitLineFadeEndDistance >= 0)
            {
                fade = true;
                orbitLine._fadeEndDist = config.Orbit.orbitLineFadeEndDistance;
            }

            orbitLine._color = color;
            lineRenderer.endColor = new Color(color.r, color.g, color.b, 0f);

            orbitLine._astroObject = astroObject;
            orbitLine._fade = fade;

            orbitLine._lineWidth = 0.2f;

            orbitLine._numVerts = (int)Mathf.Clamp(config.Orbit.semiMajorAxis / 1000f, numVerts, 4096);

            Delay.FireOnNextUpdate(orbitLine.InitializeLineRenderer);

            // If the planet has physics and a regular orbit line, make sure that when it's bumped into the old orbit line vanishes
            if (config.Base.pushable && !config.Orbit.trackingOrbitLine)
            {
                var impactSensor = planetGO.GetComponent<ImpactSensor>();
                impactSensor.OnImpact += (ImpactData _) =>
                {
                    orbitGO.SetActive(false);
                };
            }
        }
    }
}
