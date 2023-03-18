using NewHorizons.External.Modules.VariableSize;
using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class SizeController : MonoBehaviour
    {
        public AnimationCurve scaleCurve;
        public float CurrentScale { get; protected set; }
        public float size = 1f;

        public void Awake()
        {
            UpdateScale();

            if (CurrentScale == 0f)
            {
                Vanish();
            }
        }

        protected void UpdateScale()
        {
            if(scaleCurve != null)
            {
                var prevScale = CurrentScale;
                CurrentScale = scaleCurve.Evaluate(TimeLoop.GetMinutesElapsed()) * size;
                
                // #514 setting something's scale value to 0 should disable it
                if (prevScale != CurrentScale)
                {
                    if (CurrentScale == 0f)
                    {
                        Vanish();
                    }
                    else if (prevScale == 0f)
                    {
                        Appear();
                    }
                }
            }
            else
            {
                CurrentScale = size;
            }
        }

        protected virtual void Vanish()
        {
            foreach (var child in gameObject.GetAllChildren())
            {
                child.SetActive(false);
            }
        }

        protected virtual void Appear()
        {
            foreach (var child in gameObject.GetAllChildren())
            {
                child.SetActive(true);
            }
        }

        public virtual void FixedUpdate()
        {
            UpdateScale();

            transform.localScale = Vector3.one * CurrentScale;
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
