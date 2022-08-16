using NewHorizons.Components.Orbital;
using NewHorizons.External.Configs;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.General
{
    public static class AstroObjectBuilder
    {
        public static NHAstroObject Make(GameObject body, Sector sector, AstroObject primaryBody, PlanetConfig config)
        {
            NHAstroObject astroObject = body.AddComponent<NHAstroObject>();
            astroObject.HideDisplayName = !config.Base.hasMapMarker;
            astroObject.invulnerableToSun = config.Base.invulnerableToSun;

            if (config.Orbit != null) astroObject.SetOrbitalParametersFromConfig(config.Orbit);

            var type = AstroObject.Type.Planet;
            if (config.Orbit.isMoon) type = AstroObject.Type.Moon;
            // else if (config.Base.IsSatellite) type = AstroObject.Type.Satellite;
            else if (config.Base.hasCometTail) type = AstroObject.Type.Comet;
            else if (config.Star != null) type = AstroObject.Type.Star;
            else if (config.FocalPoint != null) type = AstroObject.Type.None;
            astroObject._type = type;
            astroObject._name = AstroObject.Name.CustomString;
            astroObject._customName = config.name;
            astroObject._primaryBody = primaryBody;
            astroObject._rootSector = sector;

            // Expand gravitational sphere of influence of the primary to encompass this body if needed
            if (primaryBody?.gameObject?.GetComponent<SphereCollider>() != null && !config.Orbit.isStatic)
            {
                var primarySphereOfInfluence = primaryBody.GetGravityVolume().gameObject.GetComponent<SphereCollider>();
                if (primarySphereOfInfluence.radius < config.Orbit.semiMajorAxis)
                    primarySphereOfInfluence.radius = config.Orbit.semiMajorAxis * 1.5f;
            }

            if (config.Orbit.isTidallyLocked || config.isIsland)
            {
                var alignmentAxis = config.Orbit.alignmentAxis ?? new Vector3(0, -1, 0);

                // Start it off facing the right way
                var facing = body.transform.TransformDirection(alignmentAxis);
                body.transform.rotation = Quaternion.FromToRotation(facing, alignmentAxis) * body.transform.rotation;

                var alignment = body.AddComponent<AlignWithTargetBody>();
                alignment.SetTargetBody(primaryBody?.GetAttachedOWRigidbody());
                alignment._usePhysicsToRotate = false;
                alignment._localAlignmentAxis = alignmentAxis;

                if (config.isIsland)
                {
                    alignment._degreesToTarget = 0.0198f;
                    alignment._interpolationRate = 2;
                    alignment._interpolationMode = AlignWithDirection.InterpolationMode.Linear;
                }

                // Static bodies won't update rotation with physics for some reason
                // Have to set it next tick else it flings the player into deep space on spawn (#171)
                if (!config.Orbit.isStatic) Delay.FireOnNextUpdate(() => alignment._usePhysicsToRotate = true);
            }

            if (config.Base.centerOfSolarSystem)
            {
                Logger.Log($"Setting center of universe to {config.name}");

                Delay.RunWhen(
                    () => Locator._centerOfTheUniverse != null,
                    () => Locator._centerOfTheUniverse._staticReferenceFrame = astroObject.GetComponent<OWRigidbody>()
                    );
            }

            return astroObject;
        }
    }
}
