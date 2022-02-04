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
            var original = GameObject.Find("TimberHearth_Body/Sector_TH/Interactables_TH/Geysers/Geyser_Village/");
            GameObject geyserGO = GameObject.Instantiate(original, go.transform);
            geyserGO.name = "Geyser";

            var pos = ((Vector3)info.position);
            var length = pos.magnitude;
            geyserGO.transform.localPosition = pos.normalized * (length - 100f);

            var originalRadial = -(original.transform.position - GameObject.Find("TimberHearth_Body").transform.position).normalized;
            geyserGO.transform.rotation *= Quaternion.FromToRotation(originalRadial, pos.normalized);

            geyserGO.GetComponent<GeyserController>()._inactiveDuration = 0.5f;
            
        }
    }
}
