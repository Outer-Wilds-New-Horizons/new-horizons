using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Components.SizeControllers
{
    public class SizeController : MonoBehaviour
    {
        public AnimationCurve scaleCurve { get; protected set; }
        public float CurrentScale { get; protected set; }
        public float size = 1f;

        protected void FixedUpdate()
        {
            if(scaleCurve != null)
            {
                CurrentScale = scaleCurve.Evaluate(TimeLoop.GetMinutesElapsed()) * size;
            }
            else
            {
                CurrentScale = size;
            }

            base.transform.localScale = Vector3.one * CurrentScale;
        }

        public void SetScaleCurve(TimeValuePair[] curve)
        {
            scaleCurve = new AnimationCurve();
            foreach (var pair in curve)
            {
                scaleCurve.AddKey(new Keyframe(pair.time, pair.value));
            }
        }
    }
}
