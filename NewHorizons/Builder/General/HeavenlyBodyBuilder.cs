using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
using NewHorizons.Utility;
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
        private static readonly Dictionary<string, HeavenlyBody> bodyName = new Dictionary<string, HeavenlyBody>();

        public static void Make(GameObject body, IPlanetConfig config, float SOI, GravityVolume bodyGravity, InitialMotion initialMotion)
        {
            var size = new Position.Size(config.Base.SurfaceSize, SOI);
            var G = GravityVolume.GRAVITATIONAL_CONSTANT;
            var gravity = Gravity.of(bodyGravity == null ? 2f : bodyGravity.GetFalloffExponent(), bodyGravity == null ? 0 : bodyGravity.GetStandardGravitationalParameter() / G);
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
        }

        private static HeavenlyBody AddHeavenlyBody(string name)
        {
            var hb = new HeavenlyBody(name);
            bodyName.Add(name, hb);

            var astroLookup = Position.AstroLookup;
            astroLookup.Add(hb, () => AstroObjectLocator.GetAstroObject(name));
            Position.AstroLookup = astroLookup;

            var bodyLookup = Position.BodyLookup;
            bodyLookup.Add(hb, () => AstroObjectLocator.GetAstroObject(name)?.GetAttachedOWRigidbody());
            Position.BodyLookup = bodyLookup;

            return hb;
        }

        private static HeavenlyBody GetBody(string name)
        {
            if (bodyName.ContainsKey(name))
            {
                return bodyName[name];
            }

            var hb = Position.find(AstroObjectLocator.GetAstroObject(name));
            if (hb != null)
            {
                bodyName.Add(name, hb);
            }
            return hb;
        }
    }
}
