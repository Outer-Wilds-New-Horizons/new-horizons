using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Quantum
{
    public class QuantumPlanetCompass : MonoBehaviour
    {
        [SerializeField]
        public Transform compassTransform;

        [SerializeField]
        public Transform[] stateSymbols = new Transform[0];

        [SerializeField]
        public bool loop = false;

        [SerializeField]
        public QuantumPlanet quantumPlanet;

        private float _degrees;

        public void Start()
        {
            if (compassTransform == null)
            {
                compassTransform = transform;
            }
            if (quantumPlanet == null)
            {
                enabled = false;
            }
        }

        public void Update()
        {
            int stateIndex = quantumPlanet.CurrentIndex;
            if (stateIndex < 0 || stateIndex >= stateSymbols.Length) return;

            float num = GetSymbolDegrees(stateIndex);
            if (!loop && num < 0f)
            {
                num += 360f;
            }
            else if (loop)
            {
                if (num - _degrees > 180f)
                {
                    num -= 360f;
                }
                else if (num - _degrees < -180f)
                {
                    num += 360f;
                }
            }
            _degrees = Mathf.MoveTowards(_degrees, num, Time.deltaTime * 90f);
            compassTransform.localEulerAngles = new Vector3(0f, _degrees, 0f);
        }

        private float GetSymbolDegrees(int index)
        {
            Vector3 to = Vector3.ProjectOnPlane(stateSymbols[index].transform.position - transform.position, transform.up);
            return OWMath.Angle(Vector3.ProjectOnPlane(stateSymbols[0].transform.position - transform.position, transform.up), to, transform.up);
        }
    }
}