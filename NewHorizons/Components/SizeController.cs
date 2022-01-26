using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class SizeController : MonoBehaviour
    {
        public AnimationCurve scaleCurve;
        public float CurrentScale { get; private set; }

        protected void FixedUpdate()
        {
            CurrentScale = scaleCurve.Evaluate(TimeLoop.GetMinutesElapsed());
            base.transform.localScale = new Vector3(CurrentScale, CurrentScale, CurrentScale);
        }
    }
}
