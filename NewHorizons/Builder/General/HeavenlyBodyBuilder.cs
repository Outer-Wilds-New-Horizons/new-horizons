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
        public static void Make(GameObject body, IPlanetConfig config, float SOI, GravityVolume bodyGravity, InitialMotion initialMotion)
        {
            var size = new Position.Size(config.Base.SurfaceSize, SOI);
            var G = GravityVolume.GRAVITATIONAL_CONSTANT;
            var gravity = new Gravity(G, bodyGravity == null ? 0 : bodyGravity.GetFalloffExponent(), bodyGravity == null ? 0 : bodyGravity.GetStandardGravitationalParameter() / G);
            var parent = HeavenlyBody.FromString(config.Orbit.PrimaryBody);
            var orbit = OrbitalHelper.KeplerCoordinatesFromOrbitModule(config.Orbit);

            var hb = new HeavenlyBody(config.Name);
            var planetoid = new Planet.Plantoid(size, gravity, body.transform.rotation, initialMotion._initAngularSpeed, parent, orbit);

            Planet.mapping.Add(hb, planetoid);
        }
    }
}
