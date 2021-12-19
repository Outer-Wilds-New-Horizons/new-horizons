using OWML.Utils;
using System.Reflection;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.General
{
    static class GravityBuilder
    {
        public static void Make(GameObject body, AstroObject ao, float surfaceAccel, float upperSurface, float lowerSurface)
        {
            GameObject gravityGO = new GameObject("GravityWell");
            gravityGO.transform.parent = body.transform;
            gravityGO.transform.localPosition = Vector3.zero;
            gravityGO.layer = 17;
            gravityGO.SetActive(false);

            SphereCollider SC = gravityGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            SC.radius = 2 * upperSurface;

            OWCollider OWC = gravityGO.AddComponent<OWCollider>();
            OWC.SetLODActivationMask(DynamicOccupant.Player);

            OWTriggerVolume OWTV = gravityGO.AddComponent<OWTriggerVolume>();

            GravityVolume GV = gravityGO.AddComponent<GravityVolume>();
            GV.SetValue("_cutoffAcceleration", 0.1f);
            GV.SetValue("_falloffType", GV.GetType().GetNestedType("FalloffType", BindingFlags.NonPublic).GetField("linear").GetValue(GV));
            GV.SetValue("_alignmentRadius", 0.75f * upperSurface);
            GV.SetValue("_upperSurfaceRadius", lowerSurface);
            GV.SetValue("_lowerSurfaceRadius", 0);
            GV.SetValue("_layer", 3);
            GV.SetValue("_priority", 0);
            GV.SetValue("_alignmentPriority", 0);
            GV.SetValue("_surfaceAcceleration", surfaceAccel);
            GV.SetValue("_inheritable", false);
            GV.SetValue("_isPlanetGravityVolume", true);
            GV.SetValue("_cutoffRadius", 0f);

            gravityGO.SetActive(true);

            ao.SetValue("_gravityVolume", GV);
        }
    }
}
