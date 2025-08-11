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
    public class ShipLogStellarBody : MonoBehaviour
    {
        private float _lifespan;
        private GameObject _progenitor;
        private GameObject _remnant;

        public void Initialize(float lifespan, GameObject progenitor, GameObject remnant = null)
        {
            _lifespan = lifespan;

            _progenitor = progenitor;
            _remnant = remnant;

            // Hide remnant until progenitor dies
            if (_remnant != null)
                _remnant.SetActive(false);
        }

        public void Update()
        {
            if (_lifespan <= 0) return;

            if ((TimeLoop.GetSecondsElapsed() / 60) > _lifespan)
            {
                if (_progenitor != null) _progenitor.SetActive(false);
                if (_remnant != null) _remnant.SetActive(true);
            }
        }

        public float GetDistanceFrom(ShipLogStellarBody other)
        {
            return Vector3.Distance(transform.localPosition, other.transform.localPosition);
        }
    }
}
