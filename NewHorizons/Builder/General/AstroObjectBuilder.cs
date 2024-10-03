using NewHorizons.Components;
using NewHorizons.Components.Orbital;
using NewHorizons.External;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Builder.General
{
    public static class AstroObjectBuilder
    {
        public static GameObject CenterOfUniverse { get; private set; }

        public static NHAstroObject Make(GameObject body, AstroObject primaryBody, NewHorizonsBody nhBody, bool isVanilla)
        {
            NHAstroObject astroObject = body.AddComponent<NHAstroObject>();
            astroObject.modUniqueName = nhBody.Mod.ModHelper.Manifest.UniqueName;

            var config = nhBody.Config;

            astroObject.isVanilla = isVanilla;
            astroObject.HideDisplayName = !config.MapMarker.enabled;
            astroObject.invulnerableToSun = config.Base.invulnerableToSun;

            if (config.Orbit != null) astroObject.SetOrbitalParametersFromConfig(config.Orbit);

            var type = AstroObject.Type.Planet;
            if (config.Orbit.isMoon) type = AstroObject.Type.Moon;
            // else if (config.Base.IsSatellite) type = AstroObject.Type.Satellite;
            else if (config.CometTail != null) type = AstroObject.Type.Comet;
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
                var alignmentAxis = config.Orbit.alignmentAxis ?? new Vector3(0, -1, 0);

                // Start it off facing the right way
                var alignment = body.AddComponent<AlignWithTargetBody>();
                alignment.SetTargetBody(primaryBody?.GetAttachedOWRigidbody());
                alignment._localAlignmentAxis = alignmentAxis;
                alignment._owRigidbody = body.GetComponent<OWRigidbody>();

                // Have it face the right way
                var currentDirection = alignment.transform.TransformDirection(alignment._localAlignmentAxis);
                var targetDirection = alignment.GetAlignmentDirection();
                alignment.transform.rotation = Quaternion.FromToRotation(currentDirection, targetDirection) * alignment.transform.rotation;
                alignment._owRigidbody.SetAngularVelocity(Vector3.zero);

                // Static bodies won't update rotation with physics for some reason
                alignment._usePhysicsToRotate = !config.Orbit.isStatic;
            }

            if (config.Base.centerOfSolarSystem)
            {
                CenterOfUniverse = body;

                NHLogger.Log($"Setting center of universe to {config.name}");

                Delay.RunWhen(
                    () => Locator._centerOfTheUniverse != null,
                    () => Locator._centerOfTheUniverse._staticReferenceFrame = astroObject.GetComponent<OWRigidbody>()
                );

                PreserveActiveCenterOfTheUniverse.Apply(astroObject.gameObject);
            }

            return astroObject;
        }
    }
}
