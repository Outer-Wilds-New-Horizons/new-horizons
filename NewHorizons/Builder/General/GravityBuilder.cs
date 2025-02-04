using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility.OuterWilds;
using UnityEngine;
namespace NewHorizons.Builder.General
{
    public static class GravityBuilder
    {
        public static GravityVolume Make(GameObject planetGO, AstroObject ao, OWRigidbody owrb, PlanetConfig config)
        {
            var exponent = config.Base.gravityFallOff == GravityFallOff.Linear ? 1f : 2f;
            var GM = config.Base.surfaceGravity * Mathf.Pow(config.Base.surfaceSize, exponent);

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

            var SC = gravityGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            SC.radius = gravityRadius;

            var owCollider = gravityGO.AddComponent<OWCollider>();
            owCollider.SetLODActivationMask(DynamicOccupant.Player);

            var owTriggerVolume = gravityGO.AddComponent<OWTriggerVolume>();

            // copied from th and qm
            var gravityVolume = gravityGO.AddComponent<GravityVolume>();
            gravityVolume._cutoffAcceleration = 0f;

            var falloff = config.Base.gravityFallOff == GravityFallOff.Linear? GravityVolume.FalloffType.linear : GravityVolume.FalloffType.inverseSquared;
            
            gravityVolume._falloffType = falloff;

            // Radius where your feet turn to the planet
            var alignmentRadius = config.Atmosphere?.clouds?.outerCloudRadius ?? 1.5f * config.Base.surfaceSize;
            if (config.Base.surfaceGravity == 0) alignmentRadius = 0;

            gravityVolume._alignmentRadius = config.Base.gravityAlignmentRadiusOverride ?? alignmentRadius;
            // Nobody write any FocalPoint overriding here, those work as intended gravitationally so deal with it!
            gravityVolume._upperSurfaceRadius = config.Base.surfaceSize; 
            gravityVolume._lowerSurfaceRadius = 0;
            gravityVolume._layer = 3;
            gravityVolume._priority = config.Base.gravityVolumePriority;
            gravityVolume._alignmentPriority = 0;
            gravityVolume._surfaceAcceleration = config.Base.surfaceGravity;
            gravityVolume._inheritable = false;
            gravityVolume._isPlanetGravityVolume = true;
            gravityVolume._cutoffRadius = 0f;

            gravityGO.SetActive(true);

            ao._gravityVolume = gravityVolume;
            owrb.RegisterAttachedGravityVolume(gravityVolume);

            return gravityVolume;
        }
    }
}
