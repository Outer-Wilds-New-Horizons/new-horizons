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
    static class BaseBuilder
    {
        public static Tuple<AstroObject, OWRigidbody> Make(GameObject body, AstroObject primaryBody, IPlanetConfig config)
        {
            body.AddComponent<ProxyShadowCasterSuperGroup>();

            Rigidbody rigidBody = body.AddComponent<Rigidbody>();
            rigidBody.mass = 10000;
            rigidBody.drag = 0f;
            rigidBody.angularDrag = 0f;
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            rigidBody.interpolation = RigidbodyInterpolation.None;
            rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            KinematicRigidbody kinematicRigidBody = body.AddComponent<KinematicRigidbody>();
            kinematicRigidBody.centerOfMass = Vector3.zero;

            OWRigidbody owRigidBody = body.AddComponent<OWRigidbody>();
            owRigidBody._kinematicSimulation = true;
            owRigidBody._autoGenerateCenterOfMass = true;
            owRigidBody.SetIsTargetable(true);
            owRigidBody._maintainOriginalCenterOfMass = true;
            owRigidBody._rigidbody = rigidBody;
            owRigidBody._kinematicRigidbody = kinematicRigidBody;
            owRigidBody._origParent = GameObject.Find("SolarSystemRoot").transform;
            owRigidBody.EnableKinematicSimulation();
            owRigidBody.MakeKinematic();

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
                Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => Locator.GetCenterOfTheUniverse()._staticReferenceFrame = owRigidBody, 2);
            }

            return new Tuple<AstroObject, OWRigidbody>(astroObject, owRigidBody);
        }
    }
}
