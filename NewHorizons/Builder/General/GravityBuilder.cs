using Epic.OnlineServices.Presence;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility.OuterWilds;
using System;
using UnityEngine;
namespace NewHorizons.Builder.General
{
    public static class GravityBuilder
    {
        public static GravityVolume Make(GameObject planetGO, AstroObject ao, OWRigidbody owrb, PlanetConfig config)
        {
            var exponent = config.Base.gravityFallOff == GravityFallOff.Linear ? 1f : 2f;
            var GM = config.Gravity.force * Mathf.Pow(config.Base.surfaceSize, exponent);

            // Gravity limit will be when the acceleration it would cause is less than 0.1 m/s^2
            var gravityRadius = GM / 0.1f;
            if (exponent == 2f) gravityRadius = Mathf.Sqrt(gravityRadius);

            if (config.FocalPoint != null) gravityRadius = 0; // keep it at the lowest possible
            else if (config.Base.soiOverride != 0f) gravityRadius = config.Base.soiOverride;
            else if (config.Star != null) gravityRadius = Mathf.Min(gravityRadius, 15 * config.Base.surfaceSize);
            // To let you actually orbit things the way you would expect we cap this at 4x the diameter if its not a star (this is what giants deep has)
            else gravityRadius = Mathf.Min(gravityRadius, 4 * config.Base.surfaceSize);

            var gravityGO = new GameObject("GravityWell");
            gravityGO.transform.parent = planetGO.transform;
            gravityGO.transform.localPosition = Vector3.zero;
            gravityGO.layer = Layer.BasicEffectVolume;
            gravityGO.SetActive(false);

            var sphereCollider = gravityGO.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = gravityRadius;

            var owCollider = gravityGO.AddComponent<OWCollider>();
            owCollider.SetLODActivationMask(DynamicOccupant.Player);

            var owTriggerVolume = gravityGO.AddComponent<OWTriggerVolume>();

            // copied from th and qm
            var gravityVolume = gravityGO.AddComponent<GravityVolume>();
            gravityVolume._cutoffAcceleration = config.Gravity.minForce;

            gravityVolume._falloffType = config.Gravity.fallOff switch
            {
                GravityFallOff.Linear => GravityVolume.FalloffType.linear,
                GravityFallOff.InverseSquared => GravityVolume.FalloffType.inverseSquared,
                _ => throw new NotImplementedException(),
            };

            // Radius where your feet turn to the planet
            var alignmentRadius = config.Atmosphere?.clouds?.outerCloudRadius ?? 1.5f * config.Base.surfaceSize;
            if (config.Gravity.force == 0) alignmentRadius = 0;

            gravityVolume._alignmentRadius = config.Gravity.alignmentRadius ?? alignmentRadius;
            // Nobody write any FocalPoint overriding here, those work as intended gravitationally so deal with it!
            gravityVolume._upperSurfaceRadius = config.Gravity.upperRadius; 
            gravityVolume._lowerSurfaceRadius = config.Gravity.lowerRadius;
            gravityVolume._layer = config.Gravity.layer;
            gravityVolume._priority = config.Gravity.priority;
            gravityVolume._alignmentPriority = config.Gravity.alignmentPriority;
            gravityVolume._surfaceAcceleration = config.Gravity.force;
            gravityVolume._inheritable = config.Gravity.inheritable;
            gravityVolume._isPlanetGravityVolume = true;
            gravityVolume._cutoffRadius = config.Gravity.minRadius;

            // If it's a focal point dont add collision stuff
            // This is overkill
            if (config.FocalPoint != null)
            {
                owCollider.enabled = false;
                owTriggerVolume.enabled = false;
                sphereCollider.radius = 0;
                sphereCollider.enabled = false;
                sphereCollider.isTrigger = false;
                // This should ensure that even if the player enters the volume, it counts them as being inside the zero-gee cave equivalent
                gravityVolume._cutoffRadius = gravityVolume._upperSurfaceRadius;
                gravityVolume._lowerSurfaceRadius = gravityVolume._upperSurfaceRadius;
                gravityVolume._cutoffAcceleration = 0;
            }

            gravityGO.SetActive(true);

            ao._gravityVolume = gravityVolume;
            owrb.RegisterAttachedGravityVolume(gravityVolume);

            return gravityVolume;
        }
    }
}
