using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Components
{
    public class NHMapMarker : MapMarker
    {
        public float minDisplayDistanceOverride = -1;
        public float maxDisplayDistanceOverride = -1;

        public new void Awake()
        {
            base.Awake();
            if (minDisplayDistanceOverride >= 0)
            {
                _minDisplayDistance = minDisplayDistanceOverride;
            }
            if (maxDisplayDistanceOverride >= 0)
            {
                _maxDisplayDistance = maxDisplayDistanceOverride;
            }
        }
    }
}
