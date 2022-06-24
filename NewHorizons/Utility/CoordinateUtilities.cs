using UnityEngine;
namespace NewHorizons.Utility
{
    public static class CoordinateUtilities
    {
        // Longitude and latitude are in degrees
        // Using the phi and theta convention used on https://mathworld.wolfram.com/SphericalCoordinates.html (Mathematics not physics convention)
        public static Vector3 CartesianToSpherical(Vector3 v)
        {
            // y is up in unity
            var x = v.y;
            var y = v.z;
            var z = v.x;

            float dist = Mathf.Sqrt(x * x + y * y + z * z);

            // theta
            float longitude = 180f;
            if (x > 0) longitude = Mathf.Rad2Deg * Mathf.Atan(y / x);
            if (x < 0) longitude = Mathf.Rad2Deg * (Mathf.Atan(y / x) + Mathf.PI);

            // phi
            float latitude = (Mathf.Rad2Deg * Mathf.Acos(z / dist));

            return new Vector3(longitude, latitude, dist);
        }

        public static Vector3 SphericalToCartesian(Vector3 v)
        {
            var longitude = v.x;
            var latitude = v.y;
            var r = v.z;

            var theta = Mathf.Deg2Rad * longitude;
            var phi = Mathf.Deg2Rad * latitude;

            var x = r * Mathf.Cos(theta) * Mathf.Sin(phi);
            var y = r * Mathf.Sin(theta) * Mathf.Sin(phi);
            var z = r * Mathf.Cos(phi);

            return new Vector3(z, x, y);
        }
    }
}
