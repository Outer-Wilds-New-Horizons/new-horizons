using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.External.SerializableData;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
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

            var minSize = belt.minSize;
            var maxSize = belt.maxSize;
            var count = (int)(2f * Mathf.PI * belt.innerRadius / (10f * maxSize));
            if (belt.amount >= 0) count = belt.amount;
            if (count > 200) count = 200;

            NHLogger.Log($"Generating {count} asteroid belt around {bodyName}");

            Random.InitState(belt.randomSeed);

            for (int i = 0; i < count; i++)
            {
                var size = Random.Range(minSize, maxSize);

                var config = new PlanetConfig();
                config.name = $"{bodyName} Asteroid {i}";
                config.starSystem = parentConfig.starSystem;

                config.Base = new BaseModule()
                {
                    surfaceGravity = 1,
                    surfaceSize = size,
                    gravityFallOff = GravityFallOff.InverseSquared
                };

                config.Orbit = new OrbitModule()
                {
                    isMoon = true,
                    inclination = belt.inclination + Random.Range(-2f, 2f),
                    longitudeOfAscendingNode = belt.longitudeOfAscendingNode,
                    trueAnomaly = 360f * (i + Random.Range(-0.2f, 0.2f)) / (float)count,
                    primaryBody = bodyName,
                    semiMajorAxis = Random.Range(belt.innerRadius, belt.outerRadius),
                    showOrbitLine = false
                };

                config.ReferenceFrame = new ReferenceFrameModule()
                {
                    enabled = false
                };

                config.MapMarker = new MapMarkerModule()
                {
                    enabled = false
                };

                config.ProcGen = belt.procGen;
                if (config.ProcGen == null)
                {
                    config.ProcGen = new ProcGenModule()
                    {
                        scale = size,
                        color = new MColor(126, 94, 73)
                    };
                }
                else
                {
                    // Still update the size
                    config.ProcGen.scale = size;
                }

                var asteroid = new NewHorizonsBody(config, mod);
                PlanetCreationHandler.GenerateBody(asteroid);
            }
        }
    }
}
