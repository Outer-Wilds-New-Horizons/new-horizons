using NewHorizons.Components.Orbital;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
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

            // Below is the stupid fix for making circumbinary planets or wtv

            // Grab the bodies from the main dictionary
            NewHorizonsBody primary = null;
            NewHorizonsBody secondary = null;
            foreach (var body in Main.BodyDict[Main.Instance.CurrentStarSystem])
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
                Logger.LogError($"Couldn't make focal point between [{module.primary} = {primary}] and [{module.secondary} = {secondary}]");
                return;
            }

            var gravitationalMass = GetGravitationalMass(primary.Config) + GetGravitationalMass(secondary.Config);

            // Copying it because I don't want to modify the actual config
            var fakeMassConfig = new PlanetConfig();

            // Now need to fake the 3 values to make it return this mass
            fakeMassConfig.Base.surfaceSize = 1;
            fakeMassConfig.Base.surfaceGravity = gravitationalMass * GravityVolume.GRAVITATIONAL_CONSTANT;
            fakeMassConfig.Base.gravityFallOff = primary.Config.Base.gravityFallOff;

            // Other stuff to make the fake barycenter not interact with anything in any way
            fakeMassConfig.name = config.name + "_FakeBarycenterMass";
            fakeMassConfig.Base.soiOverride = 0;
            fakeMassConfig.Base.hasMapMarker = false;
            fakeMassConfig.ReferenceFrame.hideInMap = true;

            fakeMassConfig.Orbit = new OrbitModule();
            fakeMassConfig.Orbit.CopyPropertiesFrom(config.Orbit);

            binary.FakeMassBody = PlanetCreationHandler.GenerateBody(new NewHorizonsBody(fakeMassConfig, mod));
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
