using NewHorizons.External;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class VolcanoBuilder
    {
        private static Color defaultStoneTint = new Color(0.07450981f, 0.07450981f, 0.07450981f);
        private static Color defaultLavaTint = new Color(4.594794f, 0.3419145f, 0f, 1f);

        public static void Make(GameObject go, Sector sector, PropModule.VolcanoInfo info)
        {
            var prefab = GameObject.Find("VolcanicMoon_Body/Sector_VM/Effects_VM/VolcanoPivot (2)/MeteorLauncher");

            var launcherGO = prefab.InstantiateInactive();
            launcherGO.transform.parent = sector.transform;
            launcherGO.transform.localPosition = info.position == null ? Vector3.zero : (Vector3) info.position;
            launcherGO.name = "MeteorLauncher";

            var meteorLauncher = launcherGO.GetComponent<MeteorLauncher>();
            meteorLauncher._dynamicMeteorPrefab = null;
            meteorLauncher._detectableFluid = null;
            meteorLauncher._detectableField = null;

            meteorLauncher._launchDirection = info.position == null ? Vector3.up : ((Vector3)info.position).normalized;

            var meteorPrefab = GameObject.Instantiate(meteorLauncher._meteorPrefab);
            FixMeteor(meteorPrefab, info);

            meteorLauncher._meteorPrefab = meteorPrefab;

            launcherGO.SetActive(true);

            // Kill the prefab when its done with it
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => meteorPrefab.SetActive(false));
        }
        private static void FixMeteor(GameObject meteor, PropModule.VolcanoInfo info)
        {
            var mat = meteor.GetComponentInChildren<MeshRenderer>().material;
            mat.SetColor("_Color", info.stoneTint == null ? defaultStoneTint : info.stoneTint.ToColor());
            mat.SetColor("_EmissionColor", info.lavaTint == null ? defaultLavaTint : info.lavaTint.ToColor());

            var detectors = meteor.transform.Find("ConstantDetectors");
            GameObject.Destroy(detectors.GetComponent<ConstantForceDetector>());
            GameObject.Destroy(detectors.GetComponent<ConstantFluidDetector>());

            detectors.gameObject.AddComponent<DynamicForceDetector>();
            detectors.gameObject.AddComponent<DynamicFluidDetector>();
        }
    }
}
