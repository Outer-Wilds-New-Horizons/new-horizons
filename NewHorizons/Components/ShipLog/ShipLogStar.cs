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
        internal HashSet<Vector3> usedOffsets;
        internal Transform visualGroup;
        private float scaleMultiplier;

        public void Initialize(ShipLogStarChartMode m)
        {
            mode = m;
        }

        public void Update()
        {
            if (_starTimeLoopEnd <= 0) return;
            if ((TimeLoop.GetSecondsElapsed() / 60) > _starTimeLoopEnd) gameObject.SetActive(false);
        }

        // Update the position in lateupdate so that the position isn't calculated from the previous frame if the mode was just enabled
        public void LateUpdate()
        {
            Vector3 representPosition = new Vector3(_starPosition.x - mode.cameraPosition.x, _starPosition.y - mode.cameraPosition.y, _starPosition.z) * mode.cameraZoom;
            representPosition = mode.cameraPivot.InverseTransformPoint(mode.transform.TransformPoint(representPosition));
            float Depth = (representPosition.z + 60) / 50;

            Depth = Mathf.Clamp(Depth, 0.01f, Mathf.Infinity);
            transform.localPosition = new Vector3(representPosition.x, representPosition.y, 0) / Depth;

            float minScale = 0.75f;
            float baseScale = 1f;
            float baseZoom = 8f;
            float zoomMin = 1f;
            float zoomMax = 10f;

            float maxScale = baseScale + (baseScale - minScale) * ((zoomMax - baseZoom) / (baseZoom - zoomMin));

            float t = Mathf.InverseLerp(zoomMin, zoomMax, mode.cameraZoom);
            scaleMultiplier = Mathf.Lerp(minScale, maxScale, t);

            transform.localScale = Vector3.one * (_starScale * scaleMultiplier);

        }
    }
}
