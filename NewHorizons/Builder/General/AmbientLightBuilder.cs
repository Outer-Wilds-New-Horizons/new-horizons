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
            /* R is related to the inner radius of the ambient light volume
             * G is if its a shell or not. 1.0f for shell else 0.0f.
             * B is just 1.0 always I think, altho no because changing it changes the brightness so idk
             * A is the intensity and its like square rooted and squared and idgi
             */

            light.color = new Color(0.0f, 0.0f, 0.8f, 0.0225f);
            light.range = scale;
            light.intensity = 0.5f;
        }
    }
}
