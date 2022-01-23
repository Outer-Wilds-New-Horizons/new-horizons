using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class FunnelSizeController : MonoBehaviour
    {
        public AnimationCurve scaleCurve;
        public Transform target;

        private void FixedUpdate()
        {
            float num = scaleCurve == null ? 1f : scaleCurve.Evaluate(TimeLoop.GetMinutesElapsed());
            var dist = (transform.position - target.position).magnitude;
            transform.localScale = new Vector3(num, dist/500f, num);
        }
    }
}
