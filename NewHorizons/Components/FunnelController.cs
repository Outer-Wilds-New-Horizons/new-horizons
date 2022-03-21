using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class FunnelController : MonoBehaviour
    {
        public AnimationCurve scaleCurve;
        public Transform target;
        public Transform anchor;

        private void FixedUpdate()
        {
            float num = scaleCurve == null ? 1f : scaleCurve.Evaluate(TimeLoop.GetMinutesElapsed());

            // Temporary solution that i will never get rid of
            transform.position = anchor.position;

            var dist = (transform.position - target.position).magnitude;
            transform.localScale = new Vector3(num, dist/500f, num);
        }

        private void Update()
        {
            // The target or anchor could have been destroyed by a star
            if(!target.gameObject.activeInHierarchy || !anchor.gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
