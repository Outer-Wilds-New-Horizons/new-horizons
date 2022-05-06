using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewHorizons.Components
{
    public class NHTornadoWanderController : MonoBehaviour
    {
        public float wanderRate;
        public float wanderDegreesX;
        public float wanderDegreesZ;
        public Sector sector;

        private float noiseOffset;
        private float startDegreesX;
        private float startDegreesZ;

        private float elevation;

        public void Awake()
        {
            noiseOffset = Random.value;

            var x = transform.localPosition.x;
            var y = transform.localPosition.y;
            var z = transform.localPosition.z;

            elevation = Mathf.Sqrt(x * x + y * y + z * z);

            startDegreesX = Mathf.Rad2Deg * Mathf.Atan2(y, x); //theta
            startDegreesZ = Mathf.Rad2Deg * Mathf.Acos(z / elevation); //phi
        }

        public void Update()
        {
            float num = Mathf.PerlinNoise(Time.time * wanderRate + noiseOffset, 0f) * 2f - 1f;
            float num2 = Mathf.PerlinNoise(Time.time * wanderRate + noiseOffset, 5f) * 2f - 1f;

            var newDegreesX = startDegreesX + num * wanderDegreesX;
            var newDegreesZ = startDegreesZ + num2 * wanderDegreesZ;

            var newX = elevation * Mathf.Sin(Mathf.Deg2Rad * newDegreesZ) * Mathf.Cos(Mathf.Deg2Rad * newDegreesX);
            var newY = elevation * Mathf.Sin(Mathf.Deg2Rad * newDegreesZ) * Mathf.Sin(Mathf.Deg2Rad * newDegreesX);
            var newZ = elevation * Mathf.Cos(Mathf.Deg2Rad * newDegreesZ);

            var newPos = new Vector3(newX, newY, newZ);

            transform.localPosition = newPos;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, sector.transform.TransformDirection(newPos.normalized));
        }
    }
}
