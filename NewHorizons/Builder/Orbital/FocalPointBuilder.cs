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
            binary.PrimaryName = module.Primary;
            binary.SecondaryName = module.Secondary;

            // Below is the stupid fix for making circumbinary planets or wtv

            // Grab the bodies from the main dictionary
            NewHorizonsBody primary = null;
            NewHorizonsBody secondary = null;
            foreach (var body in Main.BodyDict[Main.Instance.CurrentStarSystem])
            {
                if (body.Config.Name == module.Primary)
                {
                    primary = body;
                }
                else if (body.Config.Name == module.Secondary)
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
                Logger.LogError($"Couldn't make focal point between [{module.Primary} = {primary}] and [{module.Secondary} = {secondary}]");
                return;
            }

            var gravitationalMass = GetGravitationalMass(primary.Config) + GetGravitationalMass(secondary.Config);

            // Copying it because I don't want to modify the actual config
            var fakeMassConfig = new PlanetConfig(null);

            // Now need to fake the 3 values to make it return this mass
            fakeMassConfig.Base.SurfaceSize = 1;
            fakeMassConfig.Base.SurfaceGravity = gravitationalMass * GravityVolume.GRAVITATIONAL_CONSTANT;
            fakeMassConfig.Base.GravityFallOff = primary.Config.Base.GravityFallOff;

            // Other stuff to make the fake barycenter not interact with anything in any way
            fakeMassConfig.Name = config.Name + "_FakeBarycenterMass";
            fakeMassConfig.Base.SphereOfInfluence = 0;
            fakeMassConfig.Base.HasMapMarker = false;
            fakeMassConfig.Base.HasReferenceFrame = false;

            fakeMassConfig.Orbit = new OrbitModule();
            fakeMassConfig.Orbit.CopyPropertiesFrom(config.Orbit);

            binary.FakeMassBody = PlanetCreationHandler.GenerateBody(new NewHorizonsBody(fakeMassConfig, mod));
        }

        private static float GetGravitationalMass(PlanetConfig config)
        {
            var surfaceAcceleration = config.Base.SurfaceGravity;
            var upperSurfaceRadius = config.Base.SurfaceSize;
            int falloffExponent = config.Base.GravityFallOff.ToUpper().Equals("LINEAR") ? 1 : 2;

            return surfaceAcceleration * Mathf.Pow(upperSurfaceRadius, falloffExponent) / GravityVolume.GRAVITATIONAL_CONSTANT;
        }
    }
}
