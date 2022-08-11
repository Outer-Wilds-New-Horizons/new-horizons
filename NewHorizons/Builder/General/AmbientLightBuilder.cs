using UnityEngine;
using NewHorizons.Utility;
namespace NewHorizons.Builder.General
{
    public static class AmbientLightBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, float scale, float intensity)
        {
            var ambientLight = Main.Instance.CurrentStarSystem == "EyeOfTheUniverse" ? SearchUtilities.Find("EyeOfTheUniverse_Body/Sector_EyeOfTheUniverse/SixthPlanet_Root/QuantumMoonProxy_Pivot/QuantumMoonProxy_Root/MoonState_Root/AmbientLight_QM") : SearchUtilities.Find("BrittleHollow_Body/AmbientLight_BH_Surface");
            if (ambientLight == null) return;

            GameObject lightGO = GameObject.Instantiate(ambientLight, sector?.transform ?? planetGO.transform);
            lightGO.transform.position = planetGO.transform.position;
            lightGO.name = "AmbientLight";

            var light = lightGO.GetComponent<Light>();
            /* R is related to the inner radius of the ambient light volume
             * G is if its a shell or not. 1.0f for shell else 0.0f.
             * B is just 1.0 always I think, altho no because changing it changes the brightness so idk
             * A is the intensity and its like square rooted and squared and idgi
             */

            light.color = new Color(0.0f, 0.0f, 0.8f, 0.0225f);
            light.range = scale;
            light.intensity = intensity;
        }
    }
}
