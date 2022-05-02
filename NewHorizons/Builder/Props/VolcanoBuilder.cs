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
            launcherGO.transform.rotation = Quaternion.FromToRotation(launcherGO.transform.TransformDirection(Vector3.up), ((Vector3)info.position).normalized).normalized;
            launcherGO.name = "MeteorLauncher";

            var meteorLauncher = launcherGO.GetComponent<MeteorLauncher>();
            meteorLauncher._dynamicMeteorPrefab = null;
            meteorLauncher._detectableFluid = null;
            meteorLauncher._detectableField = null;

            meteorLauncher._launchDirection = Vector3.up;

            meteorLauncher._dynamicProbability = 0f;

            meteorLauncher._minLaunchSpeed = info.minLaunchSpeed;
            meteorLauncher._maxLaunchSpeed = info.maxLaunchSpeed;
            meteorLauncher._minInterval = info.minInterval;
            meteorLauncher._maxInterval = info.maxInterval;

            launcherGO.SetActive(true);

            // Have to null check else it breaks on reload configs
            Main.Instance.ModHelper.Events.Unity.RunWhen(() => Main.IsSystemReady && meteorLauncher._meteorPool != null, () => {
                foreach (var meteor in meteorLauncher._meteorPool)
                {
                    FixMeteor(meteor, info);
                }
            });
        }
        private static void FixMeteor(MeteorController meteor, PropModule.VolcanoInfo info)
        {
            var mat = meteor.GetComponentInChildren<MeshRenderer>().material;
            mat.SetColor("_Color", info.stoneTint == null ? defaultStoneTint : info.stoneTint.ToColor());
            mat.SetColor("_EmissionColor", info.lavaTint == null ? defaultLavaTint : info.lavaTint.ToColor());

            var detectors = meteor.transform.Find("ConstantDetectors").gameObject;
            GameObject.Destroy(detectors.GetComponent<ConstantForceDetector>());
            GameObject.Destroy(detectors.GetComponent<ConstantFluidDetector>());

            var forceDetector = detectors.gameObject.AddComponent<DynamicForceDetector>();
            detectors.gameObject.AddComponent<DynamicFluidDetector>();

            detectors.layer = LayerMask.NameToLayer("BasicDetector");

            var sphere = detectors.AddComponent<SphereCollider>();
            sphere.radius = 1;

            var sphere2 = detectors.AddComponent<SphereShape>();
            sphere2._collisionMode = Shape.CollisionMode.Detector;
            sphere2.radius = 1;

            forceDetector._collider = sphere;
            forceDetector._shape = sphere2;
        }
    }
}
