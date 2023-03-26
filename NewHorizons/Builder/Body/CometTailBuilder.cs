using NewHorizons.Components.SizeControllers;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public static class CometTailBuilder
    {
        private static GameObject _tailPrefab;

        internal static void InitPrefab()
        {
            if (_tailPrefab == null)
            {
                _tailPrefab = SearchUtilities.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes").InstantiateInactive().Rename("Prefab_CO_Tail").DontDestroyOnLoad();
            } 
        }

        public static void Make(GameObject planetGO, Sector sector, CometTailModule cometTailModule, PlanetConfig config)
        {
            var rootObj = new GameObject("CometRoot");
            rootObj.SetActive(false);
            rootObj.transform.parent = sector?.transform ?? planetGO.transform;
            rootObj.transform.localPosition = Vector3.zero;

            var cometTail = GameObject.Instantiate(_tailPrefab, rootObj.transform).Rename("CometTail");
            cometTail.transform.localPosition = Vector3.zero;
            cometTail.transform.localRotation = Quaternion.Euler(90, 90, 0);
            cometTail.SetActive(true);

            var controller = rootObj.AddComponent<CometTailController>();

            controller.size = (cometTailModule.innerRadius ?? config.Base.surfaceSize) / 110;

            if (cometTailModule.rotationOverride != null) controller.SetRotationOverride(cometTailModule.rotationOverride);

            if (string.IsNullOrEmpty(cometTailModule.primaryBody)) cometTailModule.primaryBody = config.Orbit.primaryBody;

            Delay.FireOnNextUpdate(() =>
            {
                controller.SetPrimaryBody(AstroObjectLocator.GetAstroObject(cometTailModule.primaryBody).transform);
            });

            rootObj.SetActive(true);
        }
    }
}
