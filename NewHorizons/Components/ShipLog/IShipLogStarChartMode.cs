using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.GraphicsBuffer;

namespace NewHorizons.Components.ShipLog
{
    public interface IShipLogStarChartMode
    {
        public void AddStarSystem(string uniqueID);

        public string GetTargetStarSystem();

        public void UpdateWarpPromptVisibility();
    }
}
