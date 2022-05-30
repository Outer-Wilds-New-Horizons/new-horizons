using NewHorizons.External.Configs;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.General
{
    public static class RigidBodyBuilder
    {
        public static OWRigidbody Make(GameObject body, PlanetConfig config)
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
            owRigidBody._origParent = SearchUtilities.Find("SolarSystemRoot").transform;
            owRigidBody.EnableKinematicSimulation();
            owRigidBody.MakeKinematic();

            return owRigidBody;
        }
    }
}
