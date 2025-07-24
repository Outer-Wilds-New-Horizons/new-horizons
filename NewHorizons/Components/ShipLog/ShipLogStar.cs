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
    [DefaultExecutionOrder(+50)]
    public class ShipLogStar : MonoBehaviour
    {
        public string _starName;
        public Vector3 _starPosition;
        public float _starScale;
        public bool _isWarpSystem;
        public float _starTimeLoopEnd;

        private ShipLogStarChartMode mode;

        public void Initialize(ShipLogStarChartMode m)
        {
            mode = m;
        }

        public void OnEnable()
        {
        }

        public void Update()
        {
            if ((TimeLoop.GetSecondsElapsed() / 60) > _starTimeLoopEnd) gameObject.SetActive(false);
            Vector3 representPosition = new Vector3(_starPosition.x - mode.cameraPosition.x, _starPosition.y - mode.cameraPosition.y, _starPosition.z) * mode.cameraZoom;
            representPosition = mode.cameraPivot.InverseTransformPoint(mode.transform.TransformPoint(representPosition));
            float Depth = (representPosition.z + 60) / 50;

            Depth = Mathf.Clamp(Depth, 0.01f, Mathf.Infinity);
            transform.localPosition = new Vector3(representPosition.x, representPosition.y, 0) / Depth;
            transform.localScale = Vector3.one * _starScale;
        }
    }
}
