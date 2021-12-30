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
        public static Tuple<AstroObject, OWRigidbody> Make(GameObject body, AstroObject primaryBody, Vector3 positionVector, IPlanetConfig config)
        {
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

            ParameterizedAstroObject astroObject = body.AddComponent<ParameterizedAstroObject>();

            if (config.Orbit != null) astroObject.keplerElements = KeplerElements.FromOrbitModule(config.Orbit);

            var type = AstroObject.Type.Planet;
            if (config.Orbit.IsMoon) type = AstroObject.Type.Moon;
            else if (config.Base.HasCometTail) type = AstroObject.Type.Comet;
            else if (config.Star != null) type = AstroObject.Type.Star;
            else if (config.FocalPoint != null) type = AstroObject.Type.None;
            astroObject.SetValue("_type", type);
            astroObject.SetValue("_name", AstroObject.Name.CustomString);
            astroObject.SetValue("_customName", config.Name);
            astroObject.SetValue("_primaryBody", primaryBody);

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
