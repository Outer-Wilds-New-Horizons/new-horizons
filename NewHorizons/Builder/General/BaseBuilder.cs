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
            Rigidbody RB = body.AddComponent<Rigidbody>();
            RB.mass = 10000;
            RB.drag = 0f;
            RB.angularDrag = 0f;
            RB.useGravity = false;
            RB.isKinematic = true;
            RB.interpolation = RigidbodyInterpolation.None;
            RB.collisionDetectionMode = CollisionDetectionMode.Discrete;

            KinematicRigidbody KRB = body.AddComponent<KinematicRigidbody>();
            KRB.centerOfMass = Vector3.zero;

            OWRigidbody OWRB = body.AddComponent<OWRigidbody>();
            OWRB.SetValue("_kinematicSimulation", true);
            OWRB.SetValue("_autoGenerateCenterOfMass", true);
            OWRB.SetIsTargetable(true);
            OWRB.SetValue("_maintainOriginalCenterOfMass", true);
            OWRB.SetValue("_rigidbody", RB);
            OWRB.SetValue("_kinematicRigidbody", KRB);

            AstroObject AO = body.AddComponent<AstroObject>();

            var type = AstroObject.Type.Planet;
            if (config.Orbit.IsMoon) type = AstroObject.Type.Moon;
            else if (config.Base.HasCometTail) type = AstroObject.Type.Comet;
            else if (config.Star != null) type = AstroObject.Type.Star;
            else if (config.FocalPoint != null) type = AstroObject.Type.None;
            AO.SetValue("_type", type);
            AO.SetValue("_name", AstroObject.Name.CustomString);
            AO.SetValue("_customName", config.Name);
            AO.SetValue("_primaryBody", primaryBody);

            if (config.Orbit.IsTidallyLocked)
            {
                var alignment = body.AddComponent<AlignWithTargetBody>();
                alignment.SetTargetBody(primaryBody.GetAttachedOWRigidbody());
                alignment.SetValue("_usePhysicsToRotate", true);
            }

            return new Tuple<AstroObject, OWRigidbody>(AO, OWRB);
        }
    }
}
