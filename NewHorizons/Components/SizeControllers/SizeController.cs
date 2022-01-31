using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class SizeController : MonoBehaviour
    {
        public AnimationCurve scaleCurve;
        public float CurrentScale { get; private set; }
        public float size = 1f;

        protected void FixedUpdate()
        {
            CurrentScale = scaleCurve.Evaluate(TimeLoop.GetMinutesElapsed()) * size;
            base.transform.localScale = new Vector3(CurrentScale, CurrentScale, CurrentScale);
        }
    }
}
