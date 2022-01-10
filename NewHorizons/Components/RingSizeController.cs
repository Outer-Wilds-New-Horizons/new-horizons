using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class RingSizeController : MonoBehaviour
    {
        public AnimationCurve scaleCurve;

        private void FixedUpdate()
        {
            float num = scaleCurve.Evaluate(TimeLoop.GetMinutesElapsed());
            base.transform.localScale = new Vector3(num, num, num);
        }
    }
}
