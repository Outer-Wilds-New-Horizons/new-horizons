using System.Linq;
using UnityEngine;

namespace NewHorizons.Utility
{
    internal static class VersionUtility
    {
        public static int[] RequiredVersion => new int[] {1, 1, 12};
        public static string RequiredVersionString => string.Join(".", RequiredVersion);

        public static bool CheckUpToDate()
        {
            // If they're using an outdated game version we create an error popup here
            var version = Application.version.Split('.').Select(x => int.Parse(x)).ToArray();
            var major = version[0];
            var minor = version[1];
            var patch = version[2];

            // Must be at least 1.1.12
            return major > RequiredVersion[0] || 
                (major == RequiredVersion[0] && minor > RequiredVersion[1]) || 
                (major == RequiredVersion[0] && minor == RequiredVersion[1] && patch >= RequiredVersion[2]);
        }
    }
}
