using Marshmallow.External;
using Marshmallow.Utility;
using OWML.ModHelper.Events;
using UnityEngine;

namespace Marshmallow.General
{
    static class MakeOrbitingAstroObject
    {
        public static OWRigidbody Make(GameObject body, AstroObject primaryBody, IPlanetConfig config)
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

            MakeFieldDetector.Make(body, primaryBody, config);

            AstroObject AO = body.AddComponent<AstroObject>();
            AO.SetValue("_type", AstroObject.Type.Planet);
            AO.SetValue("_name", AstroObject.Name.None);
            AO.SetPrimaryBody(primaryBody);
            if (config.HasGravity)
            {
                GravityVolume GV = MakeGravityWell.Make(body, config.SurfaceAcceleration, config.GroundSize, config.GroundSize);
                AO.SetValue("_gravityVolume", GV);
            }

            MakeOrbitLine.Make(body, AO);

            //return new Tuple(AO, rigidbody);
            return OWRB;
        }
    }
}
