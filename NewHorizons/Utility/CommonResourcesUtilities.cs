using PacificEngine.OW_CommonResources.Game.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Utility
{
    public static class CommonResourcesUtilities
    {
        public static HeavenlyBody HeavenlyBodyFromAstroObject(AstroObject obj)
        {
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
    }
}
