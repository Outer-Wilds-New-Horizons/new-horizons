using NewHorizons.External;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class GeyserBuilder
    {
        public static void Make(GameObject go, Sector sector, PropModule.GeyserInfo info)
        {
            var original = GameObject.Find("TimberHearth_Body/Sector_TH/Interactables_TH/Geysers/Geyser_Village");
            GameObject geyserGO = original.InstantiateInactive();
            geyserGO.transform.parent = sector.transform;
            geyserGO.name = "Geyser";

            var pos = ((Vector3)info.position);

            // Want half of it to be underground
            var length = pos.magnitude - 65;

            // About 130 high and the surface is at 65
            geyserGO.transform.localPosition = pos.normalized * length;

            geyserGO.transform.localScale = Vector3.one;

            var up = go.transform.TransformPoint(pos) - go.transform.position;
            geyserGO.transform.rotation = Quaternion.FromToRotation(geyserGO.transform.up, up) * geyserGO.transform.rotation;

            var controller = geyserGO.GetComponent<GeyserController>();

            geyserGO.SetActive(true);

            var geyserFluidVolume = geyserGO.GetComponentInChildren<GeyserFluidVolume>();

            // Do this after awake
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => geyserFluidVolume._maxHeight = 1);

            geyserFluidVolume.enabled = true;
            geyserGO.transform.Find("FluidVolume_Geyser").GetComponent<CapsuleShape>().enabled = true;
        }
    }
}
