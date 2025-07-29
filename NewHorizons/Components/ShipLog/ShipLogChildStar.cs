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
    public class ShipLogChildStar : MonoBehaviour
    {
        public float _starScale = 1f;
        public float _starTimeLoopEnd;

        private ShipLogStarChartMode mode;

        public void Initialize(ShipLogStarChartMode m)
        {
            mode = m;
            transform.localScale = Vector3.one * _starScale;
        }

        public void Update()
        {
            if (_starTimeLoopEnd == 0) return;
            if ((TimeLoop.GetSecondsElapsed() / 60) > _starTimeLoopEnd) gameObject.SetActive(false);
        }
    }
}
