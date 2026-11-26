using NewHorizons.Builder.Props;
using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class RepairVolumeBuilder
    {
        public static NHRepairReceiver Make(GameObject planetGO, Sector sector, RepairVolumeInfo info)
        {
            // Repair receivers aren't technically volumes (no OWTriggerVolume) so we don't use the VolumeBuilder

            var go = GeneralPropBuilder.MakeNew("RepairVolume", planetGO, ref sector, info);

            if (info.shape != null)
            {
                ShapeBuilder.AddCollider(go, info.shape);
            }
            else
            {
                var shapeInfo = new ShapeInfo()
                {
                    type = ShapeType.Sphere,
                    useShape = false,
                    hasCollision = true,
                    radius = info.radius,
                };
                ShapeBuilder.AddCollider(go, shapeInfo);
            }

            var repairReceiver = go.AddComponent<NHRepairReceiver>();
            repairReceiver.displayName = info.name ?? info.rename ?? go.name;
            repairReceiver.repairFraction = info.repairFraction;
            repairReceiver.repairTime = info.repairTime;
            repairReceiver.repairDistance = info.repairDistance;
            repairReceiver.damagedCondition = info.damagedCondition;
            repairReceiver.repairedCondition = info.repairedCondition;
            repairReceiver.revealFact = info.revealFact;

            go.SetActive(true);

            return repairReceiver;
        }
    }
}
