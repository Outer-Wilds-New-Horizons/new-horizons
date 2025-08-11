using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NewHorizons.Components.ShipLog
{
    public class ShipLogChildStar : MonoBehaviour
    {
        private float _starTimeLoopEnd;
        private GameObject _living;
        private GameObject _remnant;

        public void Initialize(float lifespan, GameObject living, GameObject remnant = null)
        {
            _starTimeLoopEnd = lifespan;

            _living = living;
            _remnant = remnant;

            // Hide remnant until star dies
            if (_remnant != null)
                _remnant.SetActive(false);
        }

        public void Update()
        {
            if (_starTimeLoopEnd <= 0) return;

            if ((TimeLoop.GetSecondsElapsed() / 60) > _starTimeLoopEnd)
            {
                if (_living != null) _living.SetActive(false);
                if (_remnant != null) _remnant.SetActive(true);
            }
        }

        public float GetDistanceFrom(ShipLogChildStar other)
        {
            return Vector3.Distance(transform.localPosition, other.transform.localPosition);
        }
    }
}
