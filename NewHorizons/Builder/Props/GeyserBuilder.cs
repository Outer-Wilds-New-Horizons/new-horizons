using NewHorizons.External.Modules;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.Props
{
    public static class GeyserBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, PropModule.GeyserInfo info)
        {
            var geyserGO = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Interactables_TH/Geysers/Geyser_Village").InstantiateInactive();
            geyserGO.transform.parent = sector?.transform ?? planetGO.transform;
            geyserGO.name = "Geyser";

            var pos = (Vector3)info.position;

            // Offset height, default -97.5 pushes it underground so the spout is at the surface
            var length = pos.magnitude + info.offset;

            // About 130 high, bubbles start at 10, shaft starts at 67, spout starts at 97.5
            geyserGO.transform.position = planetGO.transform.TransformPoint(pos.normalized * length);

            geyserGO.transform.localScale = Vector3.one;

            var up = planetGO.transform.TransformPoint(pos) - planetGO.transform.position;
            geyserGO.transform.rotation = Quaternion.FromToRotation(geyserGO.transform.up, up) * geyserGO.transform.rotation;

            if (info.disableBubbles) geyserGO.FindChild("GeyserParticles/GeyserBubbles").SetActive(false);
            if (info.disableShaft) geyserGO.FindChild("GeyserParticles/GeyserShaft").SetActive(false);
            if (info.disableSpout) geyserGO.FindChild("GeyserParticles/GeyserSpout").SetActive(false);

            geyserGO.SetActive(true);

            var geyserFluidVolume = geyserGO.GetComponentInChildren<GeyserFluidVolume>();

            // Do this after awake
            Delay.FireOnNextUpdate(() => geyserFluidVolume._maxHeight = 1);

            geyserFluidVolume.enabled = true; // why do we enable this? idk
            geyserFluidVolume.GetComponent<CapsuleShape>().enabled = true; // i think this is already enabled but wtv
        }
    }
}
