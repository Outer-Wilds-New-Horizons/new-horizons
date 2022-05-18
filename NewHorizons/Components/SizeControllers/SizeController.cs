using UnityEngine;
namespace NewHorizons.Components.SizeControllers
{
    public class SizeController : MonoBehaviour
    {
        public AnimationCurve scaleCurve;
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
    }
}
