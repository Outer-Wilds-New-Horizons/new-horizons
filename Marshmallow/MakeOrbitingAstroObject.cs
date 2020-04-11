using OWML.ModHelper.Events;
using UnityEngine;

namespace Marshmallow.General
{
    static class MakeOrbitingAstroObject
    {
        public static void Make(GameObject body, AstroObject primaryBody, float angularSpeed, bool hasGravity, float surfaceAccel, float groundSize)
        {
            Rigidbody RB = body.AddComponent<Rigidbody>();
            RB.mass = 10000;
            RB.drag = 0f;
            RB.angularDrag = 0f;
            RB.useGravity = false;
            RB.isKinematic = true;
            RB.interpolation = RigidbodyInterpolation.None;
            RB.collisionDetectionMode = CollisionDetectionMode.Discrete;

            Main.OWRB = body.AddComponent<OWRigidbody>();
            Main.OWRB.SetValue("_kinematicSimulation", true);
            Main.OWRB.SetValue("_autoGenerateCenterOfMass", true);
            Main.OWRB.SetIsTargetable(true);
            Main.OWRB.SetValue("_maintainOriginalCenterOfMass", true);

            InitialMotion IM = body.AddComponent<InitialMotion>();
            IM.SetPrimaryBody(primaryBody.GetAttachedOWRigidbody());
            IM.SetValue("_orbitAngle", 0f);
            IM.SetValue("_isGlobalAxis", false);
            IM.SetValue("_initAngularSpeed", angularSpeed);
            IM.SetValue("_initLinearSpeed", 0f);
            IM.SetValue("_isGlobalAxis", false);

            MakeFieldDetector.Make(body);

            if (hasGravity)
            {
                GravityVolume GV = MakeGravityWell.Make(body, surfaceAccel, groundSize, groundSize);
            }


            AstroObject AO = body.AddComponent<AstroObject>();
            AO.SetValue("_type", AstroObject.Type.Planet);
            AO.SetValue("_name", AstroObject.Name.InvisiblePlanet);
            AO.SetPrimaryBody(primaryBody);
            if (hasGravity)
            {
                GravityVolume GV = MakeGravityWell.Make(body, surfaceAccel, groundSize, groundSize);
                AO.SetValue("_gravityVolume", GV);
            }
        }
    }
}
