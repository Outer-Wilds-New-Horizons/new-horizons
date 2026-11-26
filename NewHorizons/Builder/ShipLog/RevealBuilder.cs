using NewHorizons.Builder.Props;
using NewHorizons.Builder.Volumes;
using NewHorizons.Components.Achievement;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.ShipLog
{
    public static class RevealBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, RevealVolumeInfo info, IModBehaviour mod)
        {
            var newRevealGO = GeneralPropBuilder.MakeNew("Reveal Volume (" + info.revealOn + ")", planetGO, ref sector, info);
            switch (info.revealOn)
            {
                case RevealVolumeInfo.RevealVolumeType.Enter:
                    MakeTrigger(newRevealGO, planetGO, sector, info, mod);
                    break;
                case RevealVolumeInfo.RevealVolumeType.Observe:
                    MakeObservable(newRevealGO, planetGO, sector, info, mod);
                    break;
                case RevealVolumeInfo.RevealVolumeType.Snapshot:
                    MakeSnapshot(newRevealGO, planetGO, sector, info, mod);
                    break;
                default:
                    NHLogger.LogError("Invalid revealOn: " + info.revealOn);
                    break;
            }
            newRevealGO.SetActive(true);
        }

        private static void MakeTrigger(GameObject go, GameObject planetGO, Sector sector, RevealVolumeInfo info, IModBehaviour mod)
        {
            var trigger = VolumeBuilder.MakeTriggerVolume<ShipLogFactListTriggerVolume>(go, planetGO, ref sector, info);

            go.layer = Layer.Default;

            if (info.reveals != null)
            {
                var factRevealVolume = go.AddComponent<ShipLogFactListTriggerVolume>();
                factRevealVolume._factIDs = info.reveals;
                switch (info.revealFor)
                {
                    case RevealVolumeInfo.EnterType.Player:
                        factRevealVolume._player = true;
                        factRevealVolume._probe = false;
                        break;
                    case RevealVolumeInfo.EnterType.Probe:
                        factRevealVolume._player = false;
                        factRevealVolume._probe = true;
                        break;
                    case RevealVolumeInfo.EnterType.Both:
                    default:
                        // if you want both player and probe to able to trigger the thing you have to set both player and probe to false. setting both to true will make nothing trigger it
                        factRevealVolume._player = false;
                        factRevealVolume._probe = false;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(info.achievementID))
            {
                var achievementVolume = go.AddComponent<AchievementVolume>();
                achievementVolume.achievementID = info.achievementID;
                switch (info.revealFor)
                {
                    case RevealVolumeInfo.EnterType.Player:
                        achievementVolume.player = true;
                        achievementVolume.probe = false;
                        break;
                    case RevealVolumeInfo.EnterType.Probe:
                        achievementVolume.player = false;
                        achievementVolume.probe = true;
                        break;
                    case RevealVolumeInfo.EnterType.Both:
                    default:
                        achievementVolume.player = true;
                        achievementVolume.probe = true;
                        break;
                }
            }
        }

        private static void MakeObservable(GameObject go, GameObject planetGO, Sector sector, RevealVolumeInfo info, IModBehaviour mod)
        {
            if (info.shape != null && info.shape?.useShape == false)
            {
                NHLogger.LogError($"Observe reveal volumes only support colliders. Affects planet [{planetGO.name}]. Set useShape to false.");
            }

            // ObserveTrigger is only compatible with colliders
            // If info.shape was null, it will still default to using a sphere with info.radius, just make sure it does so with a collider
            info.shape ??= new();
            info.shape.useShape = false;

            var collider = VolumeBuilder.MakeShapeOrCollider<ShipLogFactObserveTrigger>(go, info).GetComponent<OWCollider>();

            go.layer = Layer.Interactible;

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

        private static void MakeSnapshot(GameObject go, GameObject planetGO, Sector sector, RevealVolumeInfo info, IModBehaviour mod)
        {
            if (info.shape != null && info.shape?.useShape == false)
            {
                NHLogger.LogError($"Snapshot reveal volumes only support shapes. Affects planet [{planetGO.name}]. Set useShape to true.");
            }

            // SnapshotTrigger is only compatible with shapes
            // If info.shape was null, it will still default to using a sphere with info.radius, just make sure it does so with a shape
            info.shape ??= new();
            info.shape.useShape = true;

            var shape = VolumeBuilder.MakeShapeOrCollider<ShipLogFactSnapshotTrigger>(go, info) as Shape;
            shape.SetCollisionMode(Shape.CollisionMode.Manual);

            go.layer = Layer.Default;

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
