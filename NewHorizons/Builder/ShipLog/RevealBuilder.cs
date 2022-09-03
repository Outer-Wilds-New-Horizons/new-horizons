using NewHorizons.Components.Achievement;
using NewHorizons.External.Modules;
using OWML.Common;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.ShipLog
{
    public static class RevealBuilder
    {
        public static void Make(GameObject go, Sector sector, VolumesModule.RevealVolumeInfo info, IModBehaviour mod)
        {
            var newRevealGO = MakeGameObject(go, sector, info, mod);
            switch (info.revealOn)
            {
                case VolumesModule.RevealVolumeInfo.RevealVolumeType.Enter:
                    MakeTrigger(newRevealGO, sector, info, mod);
                    break;
                case VolumesModule.RevealVolumeInfo.RevealVolumeType.Observe:
                    MakeObservable(newRevealGO, sector, info, mod);
                    break;
                case VolumesModule.RevealVolumeInfo.RevealVolumeType.Snapshot:
                    MakeSnapshot(newRevealGO, sector, info, mod);
                    break;
                default:
                    Logger.LogError("Invalid revealOn: " + info.revealOn);
                    break;
            }
            newRevealGO.SetActive(true);
        }

        private static SphereShape MakeShape(GameObject go, VolumesModule.RevealVolumeInfo info, Shape.CollisionMode collisionMode)
        {
            SphereShape newShape = go.AddComponent<SphereShape>();
            newShape.radius = info.radius;
            newShape.SetCollisionMode(collisionMode);
            return newShape;
        }

        private static GameObject MakeGameObject(GameObject planetGO, Sector sector, VolumesModule.RevealVolumeInfo info, IModBehaviour mod)
        {
            GameObject revealTriggerVolume = new GameObject("Reveal Volume (" + info.revealOn + ")");
            revealTriggerVolume.SetActive(false);
            revealTriggerVolume.transform.parent = sector?.transform ?? planetGO.transform;

            if (!string.IsNullOrEmpty(info.parentPath))
            {
                var newParent = planetGO.transform.Find(info.parentPath);
                if (newParent != null)
                {
                    revealTriggerVolume.transform.parent = newParent;
                }
                else
                {
                    Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                }
            }

            revealTriggerVolume.transform.position = planetGO.transform.TransformPoint(info.position ?? Vector3.zero);

            return revealTriggerVolume;
        }

        private static void MakeTrigger(GameObject go, Sector sector, VolumesModule.RevealVolumeInfo info, IModBehaviour mod)
        {
            var shape = MakeShape(go, info, Shape.CollisionMode.Volume);

            var volume = go.AddComponent<OWTriggerVolume>();
            volume._shape = shape;

            if (info.reveals != null)
            {
                var factRevealVolume = go.AddComponent<ShipLogFactListTriggerVolume>();
                factRevealVolume._factIDs = info.reveals;
            }

            if (!string.IsNullOrEmpty(info.achievementID))
            {
                var achievementVolume = go.AddComponent<AchievementVolume>();
                achievementVolume.achievementID = info.achievementID;
            }
        }

        private static void MakeObservable(GameObject go, Sector sector, VolumesModule.RevealVolumeInfo info, IModBehaviour mod)
        {
            go.layer = LayerMask.NameToLayer("Interactible");

            var sphere = go.AddComponent<SphereCollider>();
            sphere.radius = info.radius;

            var collider = go.AddComponent<OWCollider>();

            var maxDistance = info.maxDistance == -1f ? 2f : info.maxDistance;

            if (info.reveals != null)
            {
                var observeTrigger = go.AddComponent<ShipLogFactObserveTrigger>();
                observeTrigger._factIDs = info.reveals;
                observeTrigger._maxViewDistance = maxDistance;
                observeTrigger._maxViewAngle = info.maxAngle;
                observeTrigger._owCollider = collider;
                observeTrigger._disableColliderOnRevealFact = true;
            }

            if (!string.IsNullOrEmpty(info.achievementID))
            {
                var achievementTrigger = go.AddComponent<AchievementObserveTrigger>();
                achievementTrigger.achievementID = info.achievementID;
                achievementTrigger.disableColliderOnUnlockAchievement = true;
                achievementTrigger.maxViewDistance = maxDistance;
                achievementTrigger.maxViewAngle = info.maxAngle;
            }
        }

        private static void MakeSnapshot(GameObject go, Sector sector, VolumesModule.RevealVolumeInfo info, IModBehaviour mod)
        {
            var shape = MakeShape(go, info, Shape.CollisionMode.Manual);

            var visibilityTracker = go.AddComponent<ShapeVisibilityTracker>();
            visibilityTracker._shapes = new Shape[] { shape };

            var maxDistance = info.maxDistance == -1f ? 200f : info.maxDistance;

            if (info.reveals != null)
            {
                var snapshotTrigger = go.AddComponent<ShipLogFactSnapshotTrigger>();
                snapshotTrigger._maxDistance = maxDistance;
                snapshotTrigger._factIDs = info.reveals;
            }

            if (!string.IsNullOrEmpty(info.achievementID))
            {
                var achievementTrigger = go.AddComponent<AchievementSnapshotTrigger>();
                achievementTrigger.maxDistance = maxDistance;
                achievementTrigger.achievementID = info.achievementID;
            }
        }
    }
}
