using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using Random = UnityEngine.Random;
namespace NewHorizons.Builder.Body
{
    public static class AsteroidBeltBuilder
    {
        public static void Make(string bodyName, PlanetConfig parentConfig, IModBehaviour mod)
        {
            var belt = parentConfig.AsteroidBelt;

            float minSize = belt.minSize;
            float maxSize = belt.maxSize;
            int count = (int)(2f * Mathf.PI * belt.innerRadius / (10f * maxSize));
            if (belt.amount >= 0) count = belt.amount;
            if (count > 200) count = 200;

            Random.InitState(belt.randomSeed);

            for (int i = 0; i < count; i++)
            {
                var size = Random.Range(minSize, maxSize);

                var config = new PlanetConfig();
                config.name = $"{bodyName} Asteroid {i}";
                config.starSystem = parentConfig.starSystem;

                config.Base = new BaseModule()
                {
                    hasMapMarker = false,
                    surfaceGravity = 1,
                    surfaceSize = size,
                    hasReferenceFrame = false,
                    gravityFallOff = GravityFallOff.InverseSquared
                };

                config.Orbit = new OrbitModule()
                {
                    IsMoon = true,
                    Inclination = belt.inclination + Random.Range(-2f, 2f),
                    LongitudeOfAscendingNode = belt.longitudeOfAscendingNode,
                    TrueAnomaly = 360f * (i + Random.Range(-0.2f, 0.2f)) / (float)count,
                    PrimaryBody = bodyName,
                    SemiMajorAxis = Random.Range(belt.innerRadius, belt.outerRadius),
                    ShowOrbitLine = false
                };

                config.ProcGen = belt.procGen;
                if (config.ProcGen == null)
                {
                    config.ProcGen = new ProcGenModule()
                    {
                        scale = size,
                        color = new MColor(126, 94, 73, 255)
                    };
                }
                else
                {
                    // Still update the size
                    config.ProcGen.scale = size;
                }

                var asteroid = new NewHorizonsBody(config, mod);
                PlanetCreationHandler.NextPassBodies.Add(asteroid);
            }
        }
    }
}
