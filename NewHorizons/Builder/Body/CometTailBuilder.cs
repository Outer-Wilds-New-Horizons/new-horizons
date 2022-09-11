using NewHorizons.External.Configs;
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

        public static void Make(GameObject planetGO, Sector sector, PlanetConfig config)
        {
            InitPrefab();

            var cometTail = GameObject.Instantiate(_tailPrefab, sector?.transform ?? planetGO.transform);
            cometTail.transform.position = planetGO.transform.position;
            cometTail.name = "CometTail";
            cometTail.transform.localScale = Vector3.one * config.Base.surfaceSize / 110;

            Vector3 alignment = new Vector3(0, 270, 90);
            if (config.Base.cometTailRotation != null) alignment = config.Base.cometTailRotation;

            cometTail.transform.rotation = Quaternion.Euler(alignment);
        }
    }
}
