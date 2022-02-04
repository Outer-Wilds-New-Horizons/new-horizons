using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.External.VariableSize
{
    public class VariableSizeModule : Module
    {
        public TimeValuePair[] Curve { get; set; }

        public class TimeValuePair
        {
            public float Time { get; set; }
            public float Value { get; set; }
        }

        public AnimationCurve ToAnimationCurve(float size = 1f)
        {
            var curve = new AnimationCurve();
            foreach (var pair in this.Curve)
            {
                curve.AddKey(new Keyframe(pair.Time, size * pair.Value));
            }
            return curve;
        }
    }
}
