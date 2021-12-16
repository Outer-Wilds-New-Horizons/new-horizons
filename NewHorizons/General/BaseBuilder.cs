using NewHorizons.External;
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

            OWRigidbody OWRB = body.AddComponent<OWRigidbody>();
            OWRB.SetValue("_kinematicSimulation", true);
            OWRB.SetValue("_autoGenerateCenterOfMass", true);
            OWRB.SetIsTargetable(true);
            OWRB.SetValue("_maintainOriginalCenterOfMass", true);
            OWRB.SetValue("_rigidbody", RB);

            InitialMotion IM = body.AddComponent<InitialMotion>();
            IM.SetPrimaryBody(primaryBody.GetAttachedOWRigidbody());
            IM.SetValue("_orbitAngle", config.Orbit.Inclination);
            IM.SetValue("_isGlobalAxis", false);
            IM.SetValue("_initAngularSpeed", config.Orbit.SiderealPeriod == 0 ? 0.02f : 1.0f / config.Orbit.SiderealPeriod);
            var orbitVector = positionVector.normalized;
            var rotationAxis = Quaternion.AngleAxis(config.Orbit.AxialTilt + config.Orbit.Inclination, orbitVector) * Vector3.up;
            IM.SetValue("_rotationAxis", rotationAxis);
            IM.SetValue("_initLinearSpeed", 0f);

            body.transform.rotation = Quaternion.FromToRotation(Vector3.up, rotationAxis);

            DetectorBuilder.Make(body, primaryBody);

            AstroObject AO = body.AddComponent<AstroObject>();
            AO.SetValue("_type", config.Orbit.IsMoon ? AstroObject.Type.Moon : AstroObject.Type.Planet);
            AO.SetValue("_name", AstroObject.Name.CustomString);
            AO.SetValue("_customName", config.Name);
            AO.SetValue("_primaryBody", primaryBody);

            if (config.Orbit.IsTidallyLocked)
            {
                // Just manually match it fr
                /*
                RotateToAstroObject RTAO = body.AddComponent<RotateToAstroObject>();
                RTAO.SetValue("_astroObjectLock", primaryBody);
                */
            }

            Logger.Log("Finished building base", Logger.LogType.Log);
            return new MTuple(AO, OWRB);
        }
    }
}
