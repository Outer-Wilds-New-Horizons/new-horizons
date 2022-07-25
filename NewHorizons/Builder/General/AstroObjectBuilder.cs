using NewHorizons.Components.Orbital;
using NewHorizons.External.Configs;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.General
{
    public static class AstroObjectBuilder
    {
        public static NHAstroObject Make(GameObject body, AstroObject primaryBody, PlanetConfig config)
        {
            NHAstroObject astroObject = body.AddComponent<NHAstroObject>();
            astroObject.HideDisplayName = !config.Base.hasMapMarker;

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

            // Expand gravitational sphere of influence of the primary to encompass this body if needed
            if (primaryBody?.gameObject?.GetComponent<SphereCollider>() != null && !config.Orbit.isStatic)
            {
                var primarySphereOfInfluence = primaryBody.GetGravityVolume().gameObject.GetComponent<SphereCollider>();
                if (primarySphereOfInfluence.radius < config.Orbit.semiMajorAxis)
                    primarySphereOfInfluence.radius = config.Orbit.semiMajorAxis * 1.5f;
            }

            if (config.Orbit.isTidallyLocked)
            {
                var alignment = body.AddComponent<AlignWithTargetBody>();
                alignment.SetTargetBody(primaryBody?.GetAttachedOWRigidbody());
                alignment._usePhysicsToRotate = !config.Orbit.isStatic;
                if (config.Orbit.alignmentAxis == null)
                {
                    alignment._localAlignmentAxis = new Vector3(0, -1, 0);
                }
                else
                {
                    alignment._localAlignmentAxis = config.Orbit.alignmentAxis;
                }
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
