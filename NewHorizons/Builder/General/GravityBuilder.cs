using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Utils;
using System;
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

            // To let you actually orbit things the way you would expect we cap this at 4x the diameter if its not a star or black hole (this is what giants deep has)
            if (config.Star != null || config.Singularity != null) gravityRadius = Mathf.Min(gravityRadius, 4 * config.Base.SurfaceSize);
            else gravityRadius = Mathf.Min(gravityRadius, 15 * config.Base.SurfaceSize);
            if (config.Base.SphereOfInfluence != 0f) gravityRadius = config.Base.SphereOfInfluence;

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

            GravityVolume.FalloffType falloff = GravityVolume.FalloffType.linear;
            if (config.Base.GravityFallOff.ToUpper().Equals("LINEAR")) falloff = GravityVolume.FalloffType.linear;
            else if (config.Base.GravityFallOff.ToUpper().Equals("INVERSESQUARED")) falloff = GravityVolume.FalloffType.inverseSquared;
            else Logger.LogError($"Couldn't set gravity type {config.Base.GravityFallOff}. Must be either \"linear\" or \"inverseSquared\". Defaulting to linear.");
            GV._falloffType = falloff;

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
