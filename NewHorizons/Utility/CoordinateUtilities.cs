using UnityEngine;
namespace NewHorizons.Utility
{
    public static class CoordinateUtilities
    {
        public static Vector3 CartesianToSpherical(Vector3 v)
        {
            float dist = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            float latitude = (Mathf.Rad2Deg * Mathf.Acos(v.z / dist));
            float longitude = 180f;
            if (v.x > 0) longitude = Mathf.Rad2Deg * Mathf.Atan(v.y / v.x);
            if (v.x < 0) longitude = Mathf.Rad2Deg * (Mathf.Atan(v.y / v.x) + Mathf.PI);

            return new Vector3(longitude, latitude, dist);
        }
    }
}
