using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class BaseBuilder
    {
        public static MTuple Make(GameObject body, AstroObject primaryBody, Vector3 positionVector, IPlanetConfig config)
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

            DetectorBuilder.Make(body, primaryBody);

            AstroObject AO = body.AddComponent<AstroObject>();
            AO.SetValue("_type", config.Orbit.IsMoon ? AstroObject.Type.Moon : AstroObject.Type.Planet);
            AO.SetValue("_name", AstroObject.Name.CustomString);
            AO.SetValue("_customName", config.Name);
            AO.SetValue("_primaryBody", primaryBody);

            if (config.Orbit.IsTidallyLocked)
            {
                var alignment = body.AddComponent<AlignWithTargetBody>();
                alignment.SetTargetBody(primaryBody.GetAttachedOWRigidbody());
                alignment.SetValue("_usePhysicsToRotate", true);
            }

            return new MTuple(AO, OWRB);
        }
    }
}
