using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.External.Configs;
using Logger = NewHorizons.Utility.Logger;
using Random = UnityEngine.Random;
using NewHorizons.Handlers;

namespace NewHorizons.Builder.Body
{
    public static class AsteroidBeltBuilder
    {
        public static void Make(string bodyName, IPlanetConfig parentConfig, IModBehaviour mod)
        {
            var belt = parentConfig.AsteroidBelt;

            float minSize = belt.MinSize;
            float maxSize = belt.MaxSize;
            int count = (int)(2f * Mathf.PI * belt.InnerRadius / (10f * maxSize));
            if (belt.Amount >= 0) count = belt.Amount;
            if (count > 200) count = 200;

            Random.InitState(belt.RandomSeed);

            for (int i = 0; i < count; i++)
            {
                var size = Random.Range(minSize, maxSize);

                var config = new PlanetConfig(null);
                config.Name = $"{bodyName} Asteroid {i}";
                config.StarSystem = parentConfig.StarSystem;

                config.Base = new BaseModule()
                {
                    HasMapMarker = false,
                    SurfaceGravity = 1,
                    SurfaceSize = size,
                    HasReferenceFrame = false,
                    GravityFallOff = "inverseSquared"
                };

                config.Orbit = new OrbitModule()
                {
                    IsMoon = true,
                    Inclination = belt.Inclination + Random.Range(-2f, 2f),
                    LongitudeOfAscendingNode = belt.LongitudeOfAscendingNode,
                    TrueAnomaly = 360f * (i + Random.Range(-0.2f, 0.2f)) / (float)count,
                    PrimaryBody = bodyName,
                    SemiMajorAxis = Random.Range(belt.InnerRadius, belt.OuterRadius),
                    ShowOrbitLine = false
                };

                config.ProcGen = belt.ProcGen;
                if(config.ProcGen == null)
                {
                    config.ProcGen = new ProcGenModule()
                    {
                        Scale = size,
                        Color = new MColor(126, 94, 73, 255)
                    };
                }
                else
                {
                    // Still update the size
                    config.ProcGen.Scale = size;
                }

                var asteroid = new NewHorizonsBody(config, mod);
                PlanetCreationHandler.NextPassBodies.Add(asteroid);
            }
        }
    }
}
