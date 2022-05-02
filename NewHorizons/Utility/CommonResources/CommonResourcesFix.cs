using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Game.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility.CommonResources
{
    public static class CommonResourcesFix
    {
        public static void Apply()
        {
            var mapping = Planet.defaultMapping;

            var DB = mapping[HeavenlyBodies.DarkBramble];
            mapping[HeavenlyBodies.DarkBramble] = new Planet.Plantoid(
                DB.size,
                DB.gravity,
                DB.state.orbit.orientation.rotation * Quaternion.AngleAxis(180, Vector3.left),
                DB.state.orbit.orientation.angularVelocity.magnitude,
                DB.state.parent,
                DB.state.orbit.coordinates
            );

            var TT = mapping[HeavenlyBodies.AshTwin];
            mapping[HeavenlyBodies.AshTwin] = new Planet.Plantoid(
                TT.size,
                TT.gravity,
                TT.state.orbit.orientation.rotation * Quaternion.AngleAxis(180, Vector3.left),
                TT.state.orbit.orientation.angularVelocity.magnitude,
                TT.state.parent,
                TT.state.orbit.coordinates
            );

            Planet.defaultMapping = mapping;
            Planet.mapping = mapping;
        }
    }
}
