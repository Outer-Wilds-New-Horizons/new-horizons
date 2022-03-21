using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Geometry.Orbits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility.CommonResources
{
    public static class CommonResourcesUtilities
    {
        public static HeavenlyBody HeavenlyBodyFromAstroObject(AstroObject obj)
        {
            if(obj == null)
            {
                Logger.LogError("Asking for a heavenly body from astro object but it is null");
            }

            switch (obj.GetAstroObjectName())
            {
                case AstroObject.Name.CustomString:
                    return HeavenlyBody.FromString(obj.GetCustomName());
                case AstroObject.Name.BrittleHollow:
                    return HeavenlyBodies.BrittleHollow;
                case AstroObject.Name.CaveTwin:
                    return HeavenlyBodies.EmberTwin;
                case AstroObject.Name.Comet:
                    return HeavenlyBodies.Interloper;
                case AstroObject.Name.DarkBramble:
                    return HeavenlyBodies.DarkBramble;
                case AstroObject.Name.DreamWorld:
                    return HeavenlyBodies.DreamWorld;
                case AstroObject.Name.GiantsDeep:
                    return HeavenlyBodies.GiantsDeep;
                case AstroObject.Name.HourglassTwins:
                    return HeavenlyBodies.HourglassTwins;
                case AstroObject.Name.MapSatellite:
                    return HeavenlyBodies.SatiliteMapping;
                case AstroObject.Name.ProbeCannon:
                    return HeavenlyBodies.ProbeCannon;
                case AstroObject.Name.QuantumMoon:
                    return HeavenlyBodies.QuantumMoon;
                case AstroObject.Name.RingWorld:
                    return HeavenlyBodies.Stranger;
                case AstroObject.Name.Sun:
                    return HeavenlyBodies.Sun;
                case AstroObject.Name.SunStation:
                    return HeavenlyBodies.SunStation;
                case AstroObject.Name.TimberHearth:
                    return HeavenlyBodies.TimberHearth;
                case AstroObject.Name.TimberMoon:
                    return HeavenlyBodies.Attlerock;
                case AstroObject.Name.TowerTwin:
                    return HeavenlyBodies.AshTwin;
                case AstroObject.Name.VolcanicMoon:
                    return HeavenlyBodies.HollowLantern;
                case AstroObject.Name.WhiteHole:
                    return HeavenlyBodies.WhiteHole;
                case AstroObject.Name.WhiteHoleTarget:
                    return HeavenlyBodies.WhiteHoleStation;
                default:
                    return HeavenlyBodies.None;
            }
        }

        public static Gravity GravityFromVolume(GravityVolume gv)
        {
            if (gv == null) return null;

            var constant = GravityVolume.GRAVITATIONAL_CONSTANT;
            var falloff = gv._falloffExponent;
            var mass = gv._gravitationalMass / constant;
            return new Gravity(constant, falloff, mass);
        }

        public static Tuple<Vector3, Vector3> GetCartesian(Gravity gravity, IKeplerCoordinates keplerCoords)
        {
            var cartesian = OrbitHelper.toCartesian(gravity, 0f, keplerCoords.GetKeplerCoords());

            // CR breaks when a = 0
            if(keplerCoords.SemiMajorAxis == 0)
            {
                cartesian = Tuple.Create(Vector3.zero, cartesian.Item2);
            }

            return cartesian;
        }

        public static Tuple<Vector3, Vector3> GetCartesian(GravityVolume gv, IKeplerCoordinates keplerCoords)
        {
            return GetCartesian(GravityFromVolume(gv), keplerCoords);
        }

        public static Vector3 GetPosition(IKeplerCoordinates keplerCoords)
        {
            if (keplerCoords.SemiMajorAxis == 0) return Vector3.zero;
            return OrbitHelper.toCartesian(new Gravity(1, 1, 1), 0f, keplerCoords.GetKeplerCoords()).Item1;
        }
    }
}
