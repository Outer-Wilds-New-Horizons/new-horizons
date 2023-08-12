using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Utility
{
    public static class RandomUtility
    {
        public static int[] GetUniqueRandomArray(int min, int max, int count)
        {
            int[] result = new int[count];
            List<int> numbersInOrder = new List<int>();
            for (var x = min; x < max; x++)
            {
                numbersInOrder.Add(x);
            }
            for (var x = 0; x < count; x++)
            {
                var randomIndex = Random.Range(0, numbersInOrder.Count);
                result[x] = numbersInOrder[randomIndex];
                numbersInOrder.RemoveAt(randomIndex);
            }

            return result;
        }

        public static List<Vector3> FibonacciSphere(int samples)
        {
            List<Vector3> points = new List<Vector3>();

            var phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

            for (int i = 0; i < samples; i++)
            {
                var y = 1 - (i / (float)(samples - 1)) * 2f;
                var radius = Mathf.Sqrt(1 - y * y);

                var theta = phi * i;

                var x = Mathf.Cos(theta) * radius;
                var z = Mathf.Sin(theta) * radius;

                points.Add(new Vector3(x, y, z));
            }
            return points;
        }
    }
}
