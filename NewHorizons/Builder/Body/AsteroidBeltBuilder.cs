using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Random = UnityEngine.Random;

namespace NewHorizons.Builder.Body
{
    static class AsteroidBeltBuilder
    {
        public static void Make(string bodyName, IPlanetConfig parentConfig, IModHelper mod)
        {
            var belt = parentConfig.AsteroidBelt;

            var minSize = 20;
            var maxSize = 50;
            int count = (int)(2f * Mathf.PI * belt.InnerRadius / (10f * maxSize));
            if (count > 200) count = 200;

            Random.InitState(belt.RandomSeed);

            for (int i = 0; i < count; i++)
            {
                var size = Random.Range(minSize, maxSize);
                var config = new Dictionary<string, object>()
                {
                    {"Name", $"{bodyName} Asteroid {i}"},
                    {"StarSystem", parentConfig.StarSystem },
                    {"Base", new Dictionary<string, object>()
                        {
                            {"HasMapMarker", false },
                            {"SurfaceGravity", 1 },
                            {"SurfaceSize", size },
                            {"HasReferenceFrame", false },
                            {"GravityFallOff", "inverseSquared" }
                        }
                    },
                    {"Orbit", new Dictionary<string, object>()
                        {
                            {"IsMoon", true },
                            {"Inclination", belt.Inclination + Random.Range(-2f, 2f) },
                            {"LongitudeOfAscendingNode", belt.LongitudeOfAscendingNode },
                            {"TrueAnomaly", 360f * (i + Random.Range(-0.2f, 0.2f)) / (float)count },
                            {"PrimaryBody", bodyName },
                            {"SemiMajorAxis", Random.Range(belt.InnerRadius, belt.OuterRadius) },
                            {"ShowOrbitLine", false }
                        }
                    },
                    {"ProcGen", new Dictionary<string, object>()
                        {
                            {"Scale", size },
                            {"Color", new MColor32(126, 94, 73, 255) }
                        }
                    }
                };

                var asteroidConfig = new PlanetConfig(config);
                if (belt.ProcGen != null) asteroidConfig.ProcGen = belt.ProcGen;
                var asteroid = new NewHorizonsBody(new PlanetConfig(config), mod);
                Main.NextPassBodies.Add(asteroid);
            }
        }
    }
}
