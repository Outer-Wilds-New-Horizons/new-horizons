using Marshmallow.External;
using Marshmallow.Utility;
using OWML.ModHelper.Events;
using UnityEngine;
using Logger = Marshmallow.Utility.Logger;

namespace Marshmallow.General
{
    static class BaseBuilder
    {
        public static MTuple Make(GameObject body, AstroObject primaryBody, IPlanetConfig config)
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
            IM.SetValue("_orbitAngle", config.OrbitAngle);
            IM.SetValue("_isGlobalAxis", false);
            IM.SetValue("_initAngularSpeed", 0.02f);
            IM.SetValue("_initLinearSpeed", 0f);

            DetectorBuilder.Make(body, primaryBody);

            AstroObject AO = body.AddComponent<AstroObject>();
            AO.SetValue("_type", AstroObject.Type.Planet);
            AO.SetValue("_name", AstroObject.Name.None);
            AO.SetValue("_primaryBody", primaryBody);
            if (config.HasGravity)
            {
                GravityVolume GV = GravityBuilder.Make(body, config.SurfaceAcceleration, config.GroundSize, config.GroundSize);
                AO.SetValue("_gravityVolume", GV);
            }

            if (config.IsTidallyLocked)
            {
                RotateToAstroObject RTAO = body.AddComponent<RotateToAstroObject>();
                RTAO.SetValue("_astroObjectLock", primaryBody);
            }

            Logger.Log("Finished building base", Logger.LogType.Log);
            return new MTuple(AO, OWRB);
        }
    }
}
