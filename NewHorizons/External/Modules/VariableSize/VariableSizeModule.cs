using Newtonsoft.Json;
using UnityEngine;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class VariableSizeModule
    {
        /// <summary>
        /// Scale this module over time
        /// </summary>
        public TimeValuePair[] Curve { get; set; }

        public AnimationCurve GetAnimationCurve(float size = 1f)
        {
            var curve = new AnimationCurve();
            if (Curve != null)
                foreach (var pair in Curve)
                    curve.AddKey(new Keyframe(pair.Time, size * pair.Value));
            return curve;
        }

        public class TimeValuePair
        {
            public float Time { get; set; }
            public float Value { get; set; }
        }
    }
}