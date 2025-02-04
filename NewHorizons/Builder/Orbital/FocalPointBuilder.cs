using NewHorizons.Components.Orbital;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.Orbital
{
    public static class FocalPointBuilder
    {
        public static void Make(GameObject go, AstroObject ao, PlanetConfig config, IModBehaviour mod)
        {
            var module = config.FocalPoint;

            var binary = go.AddComponent<BinaryFocalPoint>();
            binary.PrimaryName = module.primary;
            binary.SecondaryName = module.secondary;

            // Grab the bodies from the main dictionary
            NewHorizonsBody primary = null;
            NewHorizonsBody secondary = null;
            foreach (var body in Main.BodyDict[config.starSystem])
            {
                if (body.Config.name == module.primary)
                {
                    primary = body;
                }
                else if (body.Config.name == module.secondary)
                {
                    secondary = body;
                }
                if (primary != null && secondary != null)
                {
                    break;
                }
            }

            if (primary == null || secondary == null)
            {
                NHLogger.LogError($"Couldn't make focal point between [{module.primary} = {primary}] and [{module.secondary} = {secondary}]");
                return;
            }
        }

        public static void ValidateConfig(PlanetConfig config)
        {
            var primary = Main.BodyDict[config.starSystem].Where(x => x.Config.name == config.FocalPoint.primary).FirstOrDefault();
            var secondary = Main.BodyDict[config.starSystem].Where(x => x.Config.name == config.FocalPoint.secondary).FirstOrDefault();

            var gravitationalMass = GetGravitationalMass(primary.Config) + GetGravitationalMass(secondary.Config);

            // Now need to fake the 3 values to make it return this mass
            config.Base.surfaceSize = 1;
            config.Base.surfaceGravity = gravitationalMass * GravityVolume.GRAVITATIONAL_CONSTANT;
            config.Base.gravityFallOff = primary.Config.Base.gravityFallOff;

            // Other stuff to make the barycenter not interact with anything in any way
            config.Base.soiOverride = 0;
            var separation = primary.Config.Orbit.semiMajorAxis + secondary.Config.Orbit.semiMajorAxis;
            config.ReferenceFrame.bracketRadius = separation;
            config.ReferenceFrame.targetColliderRadius = separation;

            config.Base.showMinimap = false;
        }

        private static float GetGravitationalMass(PlanetConfig config)
        {
            var surfaceAcceleration = config.Base.surfaceGravity;
            var upperSurfaceRadius = config.Base.surfaceSize;
            int falloffExponent = config.Base.gravityFallOff == GravityFallOff.Linear ? 1 : 2;

            return surfaceAcceleration * Mathf.Pow(upperSurfaceRadius, falloffExponent) / GravityVolume.GRAVITATIONAL_CONSTANT;
        }
    }
}
