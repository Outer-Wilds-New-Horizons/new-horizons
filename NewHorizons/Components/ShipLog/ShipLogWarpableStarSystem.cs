using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NewHorizons.Components.ShipLog
{
    public class ShipLogWarpableStarSystem : ShipLogStarSystem
    {
        public string uniqueName;

        private Dictionary<string, ShipLogStellarBody> _stellarBodies = new();
        public IReadOnlyDictionary<string, ShipLogStellarBody> StellarBodies => _stellarBodies;

        public override void Initialize(ShipLogStarChartMode m)
        {
            base.Initialize(m);
            _stellarBodies = GetComponentsInChildren<ShipLogStellarBody>(true).ToDictionary(body => body.name);
        }

        public ShipLogStellarBody GetStellarBodyByName(string name)
        {
            _stellarBodies.TryGetValue(name, out var result);
            return result;
        }
    }
}
