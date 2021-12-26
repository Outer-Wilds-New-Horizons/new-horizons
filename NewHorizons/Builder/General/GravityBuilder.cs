using NewHorizons.External;
using OWML.Utils;
using System.Reflection;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    static class GravityBuilder
    {
        public static void Make(GameObject body, AstroObject ao, float surfaceAccel, float sphereOfInfluence, float surface, string falloffType)
        {
            GameObject gravityGO = new GameObject("GravityWell");
            gravityGO.transform.parent = body.transform;
            gravityGO.transform.localPosition = Vector3.zero;
            gravityGO.layer = 17;
            gravityGO.SetActive(false);

            SphereCollider SC = gravityGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            SC.radius = sphereOfInfluence;

            OWCollider OWC = gravityGO.AddComponent<OWCollider>();
            OWC.SetLODActivationMask(DynamicOccupant.Player);

            OWTriggerVolume OWTV = gravityGO.AddComponent<OWTriggerVolume>();

            GravityVolume GV = gravityGO.AddComponent<GravityVolume>();
            GV.SetValue("_cutoffAcceleration", 0.1f);
            GV.SetValue("_falloffType", GV.GetType().GetNestedType("FalloffType", BindingFlags.NonPublic).GetField(falloffType).GetValue(GV));
            GV.SetValue("_alignmentRadius", 1.5f * surface);
            GV.SetValue("_upperSurfaceRadius", surface);
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
