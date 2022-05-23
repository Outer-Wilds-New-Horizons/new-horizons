#region

using UnityEngine;

#endregion

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

            // In unity z and y are swapped compared to proper math stuff

            var x = transform.localPosition.x;
            var y = transform.localPosition.y;
            var z = transform.localPosition.z;

            elevation = Mathf.Sqrt(x * x + y * y + z * z);

            // I'm using the math notation not physics
            startDegreesX = Mathf.Rad2Deg * Mathf.Atan2(z, x); //theta (around the polar axis)
            startDegreesZ = Mathf.Rad2Deg * Mathf.Acos(y / elevation); //phi (down from the polar axis)
        }

        public void Update()
        {
            var num = Mathf.PerlinNoise(Time.time * wanderRate + noiseOffset, 0f) * 2f - 1f;
            var num2 = Mathf.PerlinNoise(Time.time * wanderRate + noiseOffset, 5f) * 2f - 1f;

            var newDegreesX = startDegreesX + num * wanderDegreesX;
            var newDegreesZ = startDegreesZ + num2 * wanderDegreesZ;

            var newX = elevation * Mathf.Sin(Mathf.Deg2Rad * newDegreesZ) * Mathf.Cos(Mathf.Deg2Rad * newDegreesX);
            var newZ = elevation * Mathf.Sin(Mathf.Deg2Rad * newDegreesZ) * Mathf.Sin(Mathf.Deg2Rad * newDegreesX);
            var newY = elevation * Mathf.Cos(Mathf.Deg2Rad * newDegreesZ);

            var newPos = new Vector3(newX, newY, newZ);

            transform.localPosition = newPos;
            transform.rotation =
                Quaternion.FromToRotation(Vector3.up, sector.transform.TransformDirection(newPos.normalized));
        }
    }
}