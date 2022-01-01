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
            var parent = AstroObjectLocator.GetAstroObject(config.Orbit.PrimaryBody);
            if (parent?.gameObject?.GetComponent<FocalPointModule>() != null)
            {
                parent = parent.GetPrimaryBody();
            }
            var a = config.Orbit.SemiMajorAxis;

            if (parent == null) 
            {
                parent = GameObject.Find("Sun_Body").GetComponent<AstroObject>();
                a = 80000;
            }

            var exponent = config.Base.GravityFallOff.Equals("linear") ? 1f : 2f;
            var m = config.Base.SurfaceGravity * Mathf.Pow(config.Base.SurfaceSize, exponent) / 0.001f;
            var M = parent.GetGravityVolume()._gravitationalMass;
            var sphereOfInfluence = a * Mathf.Pow(m / M, 2f / (3f + exponent));
            Logger.Log($"{m}, {M}, {sphereOfInfluence}, {config.Name}");

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
            GV.SetValue("_falloffType", GV.GetType().GetNestedType("FalloffType", BindingFlags.NonPublic).GetField(config.Base.GravityFallOff).GetValue(GV));
            GV.SetValue("_alignmentRadius", 1.5f * config.Base.SurfaceSize);
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
