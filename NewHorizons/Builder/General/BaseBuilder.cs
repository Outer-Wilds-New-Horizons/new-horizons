using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
using NewHorizons.Utility;
using OWML.Utils;
using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

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
            owRigidBody.SetValue("_kinematicSimulation", true);
            owRigidBody.SetValue("_autoGenerateCenterOfMass", true);
            owRigidBody.SetIsTargetable(true);
            owRigidBody.SetValue("_maintainOriginalCenterOfMass", true);
            owRigidBody.SetValue("_rigidbody", rigidBody);
            owRigidBody.SetValue("_kinematicRigidbody", kinematicRigidBody);
            owRigidBody._origParent = GameObject.Find("SolarSystemRoot").transform;
            owRigidBody.EnableKinematicSimulation();
            owRigidBody.MakeKinematic();

            ParameterizedAstroObject astroObject = body.AddComponent<ParameterizedAstroObject>();

            if (config.Orbit != null) astroObject.SetKeplerCoordinatesFromOrbitModule(config.Orbit);

            var type = AstroObject.Type.Planet;
            if (config.Orbit.IsMoon) type = AstroObject.Type.Moon;
            else if (config.Base.IsSatellite) type = AstroObject.Type.Satellite;
            else if (config.Base.HasCometTail) type = AstroObject.Type.Comet;
            else if (config.Star != null) type = AstroObject.Type.Star;
            else if (config.FocalPoint != null) type = AstroObject.Type.None;
            astroObject.SetValue("_type", type);
            astroObject.SetValue("_name", AstroObject.Name.CustomString);
            astroObject.SetValue("_customName", config.Name);
            astroObject.SetValue("_primaryBody", primaryBody);

            // Expand gravitational sphere of influence of the primary to encompass this body if needed
            if(primaryBody?.gameObject?.GetComponent<SphereCollider>() != null && !config.Orbit.IsStatic)
            {
                var primarySphereOfInfluence = primaryBody.GetGravityVolume().gameObject.GetComponent<SphereCollider>();
                if (primarySphereOfInfluence.radius < config.Orbit.SemiMajorAxis)
                    primarySphereOfInfluence.radius = config.Orbit.SemiMajorAxis * 1.5f;
            }

            if (config.Orbit.IsTidallyLocked)
            {
                var alignment = body.AddComponent<AlignWithTargetBody>();
                alignment.SetTargetBody(primaryBody?.GetAttachedOWRigidbody());
                alignment.SetValue("_usePhysicsToRotate", true);
            }

            if(config.Base.CenterOfSolarSystem)
            {
                Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => Locator.GetCenterOfTheUniverse()._staticReferenceFrame = owRigidBody, 2);
            }

            return new Tuple<AstroObject, OWRigidbody>(astroObject, owRigidBody);
        }
    }
}
