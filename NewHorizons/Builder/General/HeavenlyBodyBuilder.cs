using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
using NewHorizons.Utility;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Game.State;
using PacificEngine.OW_CommonResources.Geometry.Orbits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.General
{
    public static class HeavenlyBodyBuilder
    {
        private static readonly Dictionary<string, HeavenlyBody> _bodyMap = new Dictionary<string, HeavenlyBody>();

        public static void Make(GameObject body, IPlanetConfig config, float SOI, GravityVolume bodyGravity, InitialMotion initialMotion)
        {
            var size = new Position.Size(config.Base.SurfaceSize, SOI);

            var gravity = Gravity.of(0, 0);
            if(bodyGravity != null)
            {
                var G = GravityVolume.GRAVITATIONAL_CONSTANT;
                var exponent = bodyGravity.GetFalloffExponent();
                var mass = bodyGravity._surfaceAcceleration * Mathf.Pow(bodyGravity._upperSurfaceRadius, exponent) / G;
                gravity = Gravity.of(bodyGravity == null ? 2f : exponent, bodyGravity == null ? 0 : mass);
            }

            var parent = GetBody(config.Orbit.PrimaryBody);
            var orbit = OrbitalHelper.KeplerCoordinatesFromOrbitModule(config.Orbit);

            var hb = GetBody(config.Name);
            if (hb == null)
            {
                hb = AddHeavenlyBody(config.Name);
            }
            var planetoid = new Planet.Plantoid(size, gravity, body.transform.rotation, initialMotion._initAngularSpeed, parent, orbit);

            var mapping = Planet.defaultMapping;
            mapping[hb] = planetoid;
            Planet.defaultMapping = mapping;

            // Fix for binary focal points
            var focalPoint = Position.AstroLookup[parent].Invoke()?.gameObject.GetComponent<BinaryFocalPoint>();
            if (focalPoint != null)
            {
                var primary = Position.getBody(GetBody(focalPoint.PrimaryName));
                var secondary = Position.getBody(GetBody(focalPoint.SecondaryName));

                if(primary != null && secondary != null)
                {
                    var exponent = ((primary?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 2f) + (secondary?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 2f)) / 2f;
                    var mass = ((primary?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((primary?.GetMass() ?? 0f) * 1000f)) + (secondary?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((secondary?.GetMass() ?? 0f) * 1000f))) / 4f;

                    var currentValue = Planet.mapping[parent];
                    var newValue = new Planet.Plantoid(currentValue.size, Gravity.of(exponent, mass), currentValue.state);
                    Planet.defaultMapping[parent] = newValue;
                }
            }
        }

        private static HeavenlyBody AddHeavenlyBody(string name)
        {
            var hb = new HeavenlyBody(name);
            _bodyMap.Add(name, hb);

            var astroLookup = Position.AstroLookup;
            var bodyLookup = Position.BodyLookup;

            astroLookup.Add(hb, () => GetAstroObject(name));
            bodyLookup.Add(hb, () => GetOWRigidbody(name));

            Position.AstroLookup = astroLookup;
            Position.BodyLookup = bodyLookup;

            return hb;
        }

        private static HeavenlyBody GetBody(string name)
        {
            if (_bodyMap.ContainsKey(name))
            {
                return _bodyMap[name];
            }

            var hb = Position.find(AstroObjectLocator.GetAstroObject(name));
            if (hb != null)
            {
                _bodyMap.Add(name, hb);
            }
            return hb;
        }

        public static void OnDestroy()
        {
            Planet.defaultMapping = Planet.standardMapping;
        }

        private static AstroObject GetAstroObject(string name)
        {
            var astroBody = AstroObjectLocator.GetAstroObject(name);
            if (astroBody == null
                || astroBody.gameObject == null)
            {
                return null;
            }

            return astroBody;
        }

        private static OWRigidbody GetOWRigidbody(string name)
        {
            var astroBody = GetAstroObject(name);
            return astroBody?.GetOWRigidbody();
        }
    }
}
