using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    static class AmbientLightBuilder
    {
        public static void Make(GameObject body, float scale)
        {
            GameObject lightGO = GameObject.Instantiate(GameObject.Find("BrittleHollow_Body/AmbientLight_BH_Surface"), body.transform);
            lightGO.transform.localPosition = Vector3.zero;
            lightGO.name = "Light";

            var light = lightGO.GetComponent<Light>();
            light.name = "AmbientLight";
            light.color = new Color(0.5f, 1f, 1f, 0.0225f);
            light.range = scale;
            light.intensity = 0.5f;
        }
    }
}
