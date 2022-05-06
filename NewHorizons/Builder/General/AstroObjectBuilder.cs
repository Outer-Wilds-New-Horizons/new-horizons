using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Utils;
using System;
using UnityEngine;
using NewHorizons.External.Configs;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.Components.Orbital;

namespace NewHorizons.Builder.General
{
    static class AstroObjectBuilder
    {
        public static NHAstroObject Make(GameObject body, AstroObject primaryBody, IPlanetConfig config)
        {
            NHAstroObject astroObject = body.AddComponent<NHAstroObject>();
            astroObject.HideDisplayName = !config.Base.HasMapMarker;

            if (config.Orbit != null) astroObject.SetOrbitalParametersFromConfig(config.Orbit);

            var type = AstroObject.Type.Planet;
            if (config.Orbit.IsMoon) type = AstroObject.Type.Moon;
            else if (config.Base.IsSatellite) type = AstroObject.Type.Satellite;
            else if (config.Base.HasCometTail) type = AstroObject.Type.Comet;
            else if (config.Star != null) type = AstroObject.Type.Star;
            else if (config.FocalPoint != null) type = AstroObject.Type.None;
            astroObject._type = type;
            astroObject._name = AstroObject.Name.CustomString;
            astroObject._customName = config.Name;
            astroObject._primaryBody = primaryBody;

            // Expand gravitational sphere of influence of the primary to encompass this body if needed
            if (primaryBody?.gameObject?.GetComponent<SphereCollider>() != null && !config.Orbit.IsStatic)
            {
                var primarySphereOfInfluence = primaryBody.GetGravityVolume().gameObject.GetComponent<SphereCollider>();
                if (primarySphereOfInfluence.radius < config.Orbit.SemiMajorAxis)
                    primarySphereOfInfluence.radius = config.Orbit.SemiMajorAxis * 1.5f;
            }

            if (config.Orbit.IsTidallyLocked)
            {
                var alignment = body.AddComponent<AlignWithTargetBody>();
                alignment.SetTargetBody(primaryBody?.GetAttachedOWRigidbody());
                alignment._usePhysicsToRotate = true;
                if(config.Orbit.AlignmentAxis == null)
                {
                    alignment._localAlignmentAxis = new Vector3(0, -1, 0);
                }
                else
                {
                    alignment._localAlignmentAxis = config.Orbit.AlignmentAxis;
                }
            }

            if (config.Base.CenterOfSolarSystem)
            {
                Logger.Log($"Setting center of universe to {config.Name}");
                // By the time it runs we'll be able to get the OWRB with the method
                Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => Locator.GetCenterOfTheUniverse()._staticReferenceFrame = astroObject.GetAttachedOWRigidbody(), 2);
            }

            return astroObject;
        }
    }
}
