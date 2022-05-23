#region

using NewHorizons.External.Modules;
using OWML.Common;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

#endregion

namespace NewHorizons.Builder.ShipLog
{
    public static class RevealBuilder
    {
        public static void Make(GameObject go, Sector sector, PropModule.RevealInfo info, IModBehaviour mod)
        {
            var newRevealGO = MakeGameObject(go, sector, info, mod);
            switch (info.revealOn)
            {
                case PropModule.RevealInfo.RevealVolumeType.Enter:
                    MakeTrigger(newRevealGO, sector, info, mod);
                    break;
                case PropModule.RevealInfo.RevealVolumeType.Observe:
                    MakeObservable(newRevealGO, sector, info, mod);
                    break;
                case PropModule.RevealInfo.RevealVolumeType.Snapshot:
                    MakeSnapshot(newRevealGO, sector, info, mod);
                    break;
                default:
                    Logger.LogError("Invalid revealOn: " + info.revealOn);
                    break;
            }

            newRevealGO.SetActive(true);
        }

        private static SphereShape MakeShape(GameObject go, PropModule.RevealInfo info,
            Shape.CollisionMode collisionMode)
        {
            var newShape = go.AddComponent<SphereShape>();
            newShape.radius = info.radius;
            newShape.SetCollisionMode(collisionMode);
            return newShape;
        }

        private static GameObject MakeGameObject(GameObject planetGO, Sector sector, PropModule.RevealInfo info,
            IModBehaviour mod)
        {
            var revealTriggerVolume = new GameObject("Reveal Volume (" + info.revealOn + ")");
            revealTriggerVolume.SetActive(false);
            revealTriggerVolume.transform.parent = sector?.transform ?? planetGO.transform;
            revealTriggerVolume.transform.position = planetGO.transform.TransformPoint(info.position ?? Vector3.zero);
            return revealTriggerVolume;
        }

        private static void MakeTrigger(GameObject go, Sector sector, PropModule.RevealInfo info, IModBehaviour mod)
        {
            var newShape = MakeShape(go, info, Shape.CollisionMode.Volume);
            var newVolume = go.AddComponent<OWTriggerVolume>();
            newVolume._shape = newShape;
            var volume = go.AddComponent<ShipLogFactListTriggerVolume>();
            volume._factIDs = info.reveals;
        }

        private static void MakeObservable(GameObject go, Sector sector, PropModule.RevealInfo info, IModBehaviour mod)
        {
            go.layer = LayerMask.NameToLayer("Interactible");
            var newSphere = go.AddComponent<SphereCollider>();
            newSphere.radius = info.radius;
            var newCollider = go.AddComponent<OWCollider>();
            var newObserveTrigger = go.AddComponent<ShipLogFactObserveTrigger>();
            newObserveTrigger._factIDs = info.reveals;
            newObserveTrigger._maxViewDistance = info.maxDistance == -1f ? 2f : info.maxDistance;
            newObserveTrigger._maxViewAngle = info.maxAngle;
            newObserveTrigger._owCollider = newCollider;
            newObserveTrigger._disableColliderOnRevealFact = true;
        }

        private static void MakeSnapshot(GameObject go, Sector sector, PropModule.RevealInfo info, IModBehaviour mod)
        {
            var newShape = MakeShape(go, info, Shape.CollisionMode.Manual);
            var newTracker = go.AddComponent<ShapeVisibilityTracker>();
            newTracker._shapes = new Shape[] {newShape};
            var newSnapshotTrigger = go.AddComponent<ShipLogFactSnapshotTrigger>();
            newSnapshotTrigger._maxDistance = info.maxDistance == -1f ? 200f : info.maxDistance;
            newSnapshotTrigger._factIDs = info.reveals;
        }
    }
}