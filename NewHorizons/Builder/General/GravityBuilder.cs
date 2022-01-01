using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Utils;
using System.Reflection;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    static class GravityBuilder
    {
        public static GravityVolume Make(GameObject body, AstroObject ao, IPlanetConfig config)
        {
            var exponent = config.Base.GravityFallOff.Equals("linear") ? 1f : 2f;
            var GM = config.Base.SurfaceGravity * Mathf.Pow(config.Base.SurfaceSize, exponent);

            // Gravity limit will be when the acceleration it would cause is less than 0.1 m/s^2
            var gravityRadius = GM / 0.1f;
            if (exponent == 2f) gravityRadius = Mathf.Sqrt(gravityRadius);

            GameObject gravityGO = new GameObject("GravityWell");
            gravityGO.transform.parent = body.transform;
            gravityGO.transform.localPosition = Vector3.zero;
            gravityGO.layer = 17;
            gravityGO.SetActive(false);

            SphereCollider SC = gravityGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            SC.radius = gravityRadius;

            OWCollider OWC = gravityGO.AddComponent<OWCollider>();
            OWC.SetLODActivationMask(DynamicOccupant.Player);

            OWTriggerVolume OWTV = gravityGO.AddComponent<OWTriggerVolume>();

            GravityVolume GV = gravityGO.AddComponent<GravityVolume>();
            GV.SetValue("_cutoffAcceleration", 0.1f);
            GV.SetValue("_falloffType", GV.GetType().GetNestedType("FalloffType", BindingFlags.NonPublic).GetField(config.Base.GravityFallOff).GetValue(GV));
            GV.SetValue("_alignmentRadius", config.Base.SurfaceGravity != 0 ? 1.5f * config.Base.SurfaceSize : 0f);
            GV.SetValue("_upperSurfaceRadius", config.Base.SurfaceSize);
            GV.SetValue("_lowerSurfaceRadius", 0);
            GV.SetValue("_layer", 3);
            GV.SetValue("_priority", 0);
            GV.SetValue("_alignmentPriority", 0);
            GV.SetValue("_surfaceAcceleration", config.Base.SurfaceGravity);
            GV.SetValue("_inheritable", false);
            GV.SetValue("_isPlanetGravityVolume", true);
            GV.SetValue("_cutoffRadius", 0f);

            gravityGO.SetActive(true);

            ao.SetValue("_gravityVolume", GV);

            return GV;
        }
    }
}
