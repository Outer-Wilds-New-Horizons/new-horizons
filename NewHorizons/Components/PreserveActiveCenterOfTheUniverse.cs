using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Components
{
    // Prevents the center of the universe being deactivated
    public class PreserveActiveCenterOfTheUniverse : MonoBehaviour
    {
        private GameObject _centerOfTheUniverse;

        public static void Apply(GameObject center)
        {
            var go = new GameObject(nameof(PreserveActiveCenterOfTheUniverse));
            go.AddComponent<PreserveActiveCenterOfTheUniverse>()._centerOfTheUniverse = center;
        }

        public void Update()
        {
            if (!_centerOfTheUniverse.activeInHierarchy && !CenterOfTheUniverse.IsUniverseDeactivated())
            {
                NHLogger.LogWarning("Center of the universe cannot be inactive until the universe is deactivated.");
                _centerOfTheUniverse.SetActive(true);
            }
        }
    }
}
