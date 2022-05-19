using NewHorizons.External.Modules;
using OWML.Common;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.ShipLog
{
    public static class RevealBuilder
    {
        public static void Make(GameObject go, Sector sector, PropModule.RevealInfo info, IModBehaviour mod)
        {
            GameObject newRevealGO = MakeGameObject(go, sector, info, mod);
            switch (info.revealOn.ToLower())
            {
                case "enter":
                    MakeTrigger(newRevealGO, sector, info, mod);
                    break;
                case "observe":
                    MakeObservable(newRevealGO, sector, info, mod);
                    break;
                case "snapshot":
                    MakeSnapshot(newRevealGO, sector, info, mod);
                    break;
                default:
                    Logger.LogError("Invalid revealOn: " + info.revealOn);
                    break;
            }

            newRevealGO.SetActive(true);
        }

        private static SphereShape MakeShape(GameObject go, PropModule.RevealInfo info, Shape.CollisionMode collisionMode)
        {
            SphereShape newShape = go.AddComponent<SphereShape>();
            newShape.radius = info.radius;
            newShape.SetCollisionMode(collisionMode);
            return newShape;
        }

        private static GameObject MakeGameObject(GameObject planetGO, Sector sector, PropModule.RevealInfo info, IModBehaviour mod)
        {
            GameObject revealTriggerVolume = new GameObject("Reveal Volume (" + info.revealOn + ")");
            revealTriggerVolume.SetActive(false);
            revealTriggerVolume.transform.parent = sector?.transform ?? planetGO.transform;
            revealTriggerVolume.transform.position = planetGO.transform.TransformPoint(info.position ?? Vector3.zero);
            return revealTriggerVolume;
        }

        private static void MakeTrigger(GameObject go, Sector sector, PropModule.RevealInfo info, IModBehaviour mod)
        {
            SphereShape newShape = MakeShape(go, info, Shape.CollisionMode.Volume);
            OWTriggerVolume newVolume = go.AddComponent<OWTriggerVolume>();
            newVolume._shape = newShape;
            ShipLogFactListTriggerVolume volume = go.AddComponent<ShipLogFactListTriggerVolume>();
            volume._factIDs = info.reveals;
        }

        private static void MakeObservable(GameObject go, Sector sector, PropModule.RevealInfo info, IModBehaviour mod)
        {
            go.layer = LayerMask.NameToLayer("Interactible");
            SphereCollider newSphere = go.AddComponent<SphereCollider>();
            newSphere.radius = info.radius;
            OWCollider newCollider = go.AddComponent<OWCollider>();
            ShipLogFactObserveTrigger newObserveTrigger = go.AddComponent<ShipLogFactObserveTrigger>();
            newObserveTrigger._factIDs = info.reveals;
            newObserveTrigger._maxViewDistance = info.maxDistance == -1f ? 2f : info.maxDistance;
            newObserveTrigger._maxViewAngle = info.maxAngle;
            newObserveTrigger._owCollider = newCollider;
            newObserveTrigger._disableColliderOnRevealFact = true;
        }

        private static void MakeSnapshot(GameObject go, Sector sector, PropModule.RevealInfo info, IModBehaviour mod)
        {
            SphereShape newShape = MakeShape(go, info, Shape.CollisionMode.Manual);
            ShapeVisibilityTracker newTracker = go.AddComponent<ShapeVisibilityTracker>();
            newTracker._shapes = new Shape[] { newShape };
            ShipLogFactSnapshotTrigger newSnapshotTrigger = go.AddComponent<ShipLogFactSnapshotTrigger>();
            newSnapshotTrigger._maxDistance = info.maxDistance == -1f ? 200f : info.maxDistance;
            newSnapshotTrigger._factIDs = info.reveals;
        }
    }
}
