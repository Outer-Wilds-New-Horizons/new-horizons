using NewHorizons.Builder.Atmosphere;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components
{
    public class EyeSunLightParamUpdater : MonoBehaviour
    {
        public static readonly int SunPosition = Shader.PropertyToID("_SunPosition");
        public static readonly int OWSunPositionRange = Shader.PropertyToID("_OWSunPositionRange");
        public static readonly int OWSunColorIntensity = Shader.PropertyToID("_OWSunColorIntensity");
        public static readonly Vector3 position = new Vector3(0, 1, -10) * 1000000;
        public static readonly Vector4 color = new Vector4(0.3569f, 0.7843f, 1, 0.2f);
        public static readonly float radius = 100;
        public static readonly float range = 100000;

        public void LateUpdate()
        {
            Shader.SetGlobalVector(SunPosition, new Vector4(position.x, position.y, position.z, radius));
            Shader.SetGlobalVector(OWSunPositionRange, new Vector4(position.x, position.y, position.z, range * range));
            Shader.SetGlobalVector(OWSunColorIntensity, color);
        }
    }
}
