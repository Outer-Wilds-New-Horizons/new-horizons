using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
using NewHorizons.Utility;
using OWML.Common;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Game.State;
using PacificEngine.OW_CommonResources.Geometry.Orbits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    public static class HeavenlyBodyBuilder
    {
        private static readonly Dictionary<string, HeavenlyBody> _bodyMap = new Dictionary<string, HeavenlyBody>();

        public static void Make(GameObject body, IPlanetConfig config, float SOI, GravityVolume bodyGravity, InitialMotion initialMotion)
        {
            var size = new Position.Size(config.Base.SurfaceSize, SOI);
            var gravity = getGravity(bodyGravity);
            var parent = GetBody(config.Orbit.PrimaryBody);
            var orbit = OrbitalHelper.KeplerCoordinatesFromOrbitModule(config.Orbit);
            if (parent == HeavenlyBody.None)
            {
                Helper.helper.Console.WriteLine($"Could not find planet ({config.Name}) reference to its parent {config.Orbit.PrimaryBody}", MessageType.Warning);
            }

            var hb = GetBody(config.Name);
            if (hb == null)
            {
                hb = AddHeavenlyBody(config.Name);
            }

            Planet.Plantoid planetoid;
            
            if (!config.Orbit.IsStatic)
            {
                planetoid = new Planet.Plantoid(size, gravity, body.transform.rotation, initialMotion._initAngularSpeed, parent, orbit);
            }
            else
            {
                planetoid = new Planet.Plantoid(size, gravity, body.transform.rotation, 0f, HeavenlyBody.None, body.transform.position, Vector3.zero);
            }

            var mapping = Planet.defaultMapping;
            mapping[hb] = planetoid;
            Planet.defaultMapping = mapping;

            // Fix for binary focal points
            var focalPoint = Position.AstroLookup[parent].Invoke()?.gameObject.GetComponent<BinaryFocalPoint>();
            if (focalPoint != null && mapping.ContainsKey(parent))
            {
                var primary = Position.getBody(GetBody(focalPoint.PrimaryName));
                var secondary = Position.getBody(GetBody(focalPoint.SecondaryName));

                if(primary != null && secondary != null)
                {
                    Logger.Log($"Fixing BinaryFocalPoint HeavenlyBody gravity value for {parent.name}");
                    var primaryGravity = getGravity(primary?.GetAttachedGravityVolume());
                    var secondaryGravity = getGravity(secondary?.GetAttachedGravityVolume());

                    var exponent = (primaryGravity.exponent + secondaryGravity.exponent) / 2f;
                    var mass = (primaryGravity.mass + secondaryGravity.mass) / 4f;

                    mapping[parent] = new Planet.Plantoid(mapping[parent].size, Gravity.of(exponent, mass), mapping[parent].state);
                    Planet.defaultMapping = mapping;
                }
            }
        }

        public static void Remove(AstroObject obj)
        {
            var astro = Position.find(obj);

            var mapping = Planet.defaultMapping;
            mapping.Remove(astro);
            Planet.defaultMapping = mapping;
        }

        private static Gravity getGravity(GravityVolume volume)
        {
            if (volume == null)
            {
                return Gravity.of(2, 0);
            }

            var exponent = volume._falloffType != GravityVolume.FalloffType.linear ? 2f : 1f;
            var mass = (volume._surfaceAcceleration * Mathf.Pow(volume._upperSurfaceRadius, exponent)) / GravityVolume.GRAVITATIONAL_CONSTANT;

            return Gravity.of(exponent, mass);
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

        public static void Reset()
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
