using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public static class CometTailBuilder
    {
        private static GameObject _tailPrefab;

        internal static void InitPrefab()
        {
            if (_tailPrefab == null) _tailPrefab = SearchUtilities.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes").InstantiateInactive().Rename("Prefab_CO_Tail").DontDestroyOnLoad();
        }

        public static void Make(GameObject planetGO, Sector sector, CometTailModule cometTailModule, float surfaceSize)
        {
            var cometTail = GameObject.Instantiate(_tailPrefab, sector?.transform ?? planetGO.transform);
            cometTail.transform.position = planetGO.transform.position;
            cometTail.name = "CometTail";
            cometTail.transform.localScale = Vector3.one * (cometTailModule.innerRadius ?? surfaceSize) / 110;

            var alignment = new Vector3(0, 270, 90);
            if (cometTailModule.rotationOverride != null) alignment = cometTailModule.rotationOverride;

            cometTail.transform.rotation = Quaternion.Euler(alignment);

            cometTail.SetActive(true);
        }
    }
}
