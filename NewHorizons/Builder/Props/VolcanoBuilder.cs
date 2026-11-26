using NewHorizons.External.Modules.Props;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class VolcanoBuilder
    {
        private static Color defaultStoneTint = new Color(0.07450981f, 0.07450981f, 0.07450981f);
        private static Color defaultLavaTint = new Color(4.594794f, 0.3419145f, 0f, 1f);
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private static GameObject _meteorLauncherPrefab;

        internal static void InitPrefab()
        {
            if (_meteorLauncherPrefab == null)
            {
                _meteorLauncherPrefab = SearchUtilities.Find("VolcanicMoon_Body/Sector_VM/Effects_VM/VolcanoPivot (2)/MeteorLauncher").InstantiateInactive().Rename("Prefab_VM_MeteorLauncher").DontDestroyOnLoad();
                var meteorLauncher = _meteorLauncherPrefab.GetComponent<MeteorLauncher>();
                meteorLauncher._audioSector = null;
                meteorLauncher._dynamicMeteorPrefab = null;
                meteorLauncher._detectableFluid = null;
                meteorLauncher._detectableField = null;
                meteorLauncher._launchDirection = Vector3.up;
                meteorLauncher._dynamicProbability = 0f;
                var meteorPrefab = meteorLauncher._meteorPrefab.InstantiateInactive().Rename("Prefab_VM_MoltenMeteor").DontDestroyOnLoad();
                var meteor = meteorPrefab.GetComponent<MeteorController>();
                Object.DestroyImmediate(meteorPrefab.FindChild("ConstantDetectors"));
                var detectors = meteorPrefab.FindChild("DynamicDetector");
                var rigidbody = meteor.GetComponent<OWRigidbody>();
                meteor._owRigidbody = rigidbody;
                meteor._constantFluidDetector = null;
                meteor._constantForceDetector = null;
                rigidbody.RegisterAttachedFluidDetector(detectors.GetComponent<DynamicFluidDetector>());
                rigidbody.RegisterAttachedForceDetector(detectors.AddComponent<DynamicForceDetector>());
                meteor._owColliders = meteorPrefab.GetComponentsInChildren<OWCollider>();
                meteorLauncher._meteorPrefab = meteorPrefab;
            }
        }

        public static void Make(GameObject planetGO, Sector sector, VolcanoInfo info)
        {
            InitPrefab();

            var launcherGO = GeneralPropBuilder.MakeFromPrefab(_meteorLauncherPrefab, "MeteorLauncher", planetGO, ref sector, info);

            var meteorLauncher = launcherGO.GetComponent<MeteorLauncher>();
            meteorLauncher._audioSector = sector;
            meteorLauncher._minLaunchSpeed = info.minLaunchSpeed;
            meteorLauncher._maxLaunchSpeed = info.maxLaunchSpeed;
            meteorLauncher._minInterval = info.minInterval;
            meteorLauncher._maxInterval = info.maxInterval;

            var lavaEruption = launcherGO.FindChild("EruptionParticles_Lava").GetComponent<ParticleSystemRenderer>();
            var lavaMaterial = new Material(lavaEruption.sharedMaterial);
            lavaMaterial.SetColor(Color1, info.stoneTint?.ToColor() ?? defaultStoneTint);
            lavaMaterial.SetColor(EmissionColor, info.lavaTint?.ToColor() ?? defaultLavaTint);
            lavaEruption.sharedMaterial = lavaMaterial;

            launcherGO.SetActive(true);

            // Have to null check else it breaks on reload configs
            Delay.RunWhen(() => Main.IsSystemReady && meteorLauncher._meteorPool != null, () =>
            {
                foreach (var meteor in meteorLauncher._meteorPool)
                {
                    FixMeteor(meteor, info);
                }
            });
        }

        private static void FixMeteor(MeteorController meteor, VolcanoInfo info)
        {
            meteor.transform.localScale = Vector3.one * info.scale;

            var meteorRenderer = meteor.GetComponentInChildren<MeshRenderer>();
            var mat = new Material(meteorRenderer.sharedMaterial);
            mat.SetColor(Color1, info.stoneTint?.ToColor() ?? defaultStoneTint);
            mat.SetColor(EmissionColor, info.lavaTint?.ToColor() ?? defaultLavaTint);
            meteorRenderer.sharedMaterial = mat;

            foreach (var p in meteor.gameObject.FindChild("Effects_VM_MeteorParticles").GetComponentsInChildren<ParticleSystemRenderer>())
            {
                if (p.name.Contains("Shrapnel")) p.sharedMaterial = mat;
            }
        }
    }
}
