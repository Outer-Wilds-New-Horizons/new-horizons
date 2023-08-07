using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.General
{
    public static class RigidBodyBuilder
    {
        public static OWRigidbody Make(GameObject body, float sphereOfInfluence)
        {
            body.AddComponent<ProxyShadowCasterSuperGroup>()._bounds.radius = sphereOfInfluence * 2;

            Rigidbody rigidBody = body.AddComponent<Rigidbody>();
            rigidBody.mass = 10000;
            rigidBody.drag = 0f;
            rigidBody.angularDrag = 0f;
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            rigidBody.interpolation = RigidbodyInterpolation.None;
            rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            KinematicRigidbody kinematicRigidBody = body.AddComponent<KinematicRigidbody>();

            OWRigidbody owRigidBody = body.AddComponent<OWRigidbody>();
            owRigidBody._kinematicSimulation = true;
            owRigidBody._autoGenerateCenterOfMass = true;
            owRigidBody.SetIsTargetable(true);
            owRigidBody._maintainOriginalCenterOfMass = true;
            owRigidBody._rigidbody = rigidBody;
            owRigidBody._kinematicRigidbody = kinematicRigidBody;
            owRigidBody._origParent = SearchUtilities.Find("SolarSystemRoot")?.transform;
            owRigidBody.EnableKinematicSimulation();
            owRigidBody.MakeKinematic();

            return owRigidBody;
        }
    }
}
