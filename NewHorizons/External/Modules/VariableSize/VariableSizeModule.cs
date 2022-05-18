using UnityEngine;
namespace NewHorizons.External.Modules.VariableSize
{
    public class VariableSizeModule 
    {
        public TimeValuePair[] Curve { get; set; }

        public class TimeValuePair
        {
            public float Time { get; set; }
            public float Value { get; set; }
        }

        public AnimationCurve GetAnimationCurve(float size = 1f)
        {
            var curve = new AnimationCurve();
            if(Curve != null)
            {
                foreach (var pair in this.Curve)
                {
                    curve.AddKey(new Keyframe(pair.Time, size * pair.Value));
                }
            }
            return curve;
        }
    }
}
