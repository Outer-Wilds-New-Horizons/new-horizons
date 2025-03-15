using NewHorizons.Components.SizeControllers;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public static class CometTailBuilder
    {
        private static GameObject _dustPrefab;
        private static GameObject _gasPrefab;

        internal static void InitPrefab()
        {
            if (_dustPrefab == null)
            {
                _dustPrefab = new GameObject("Prefab_CO_Dust").DontDestroyOnLoad();

                var dust1 = SearchUtilities.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes/Effects_CO_DustTail").Instantiate();
                dust1.transform.parent = _dustPrefab.transform;
                dust1.transform.localPosition = Vector3.zero;
                dust1.transform.localRotation = Quaternion.Euler(0, 270, 0);

                var dust2 = SearchUtilities.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes/Effects_CO_DustTail (1)").Instantiate();
                dust2.transform.parent = _dustPrefab.transform;
                dust2.transform.localPosition = Vector3.zero;
                dust2.transform.localRotation = Quaternion.Euler(0, 270, 0);

                var dust3 = SearchUtilities.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes/Effects_CO_DustTail (2)").Instantiate();
                dust3.transform.parent = _dustPrefab.transform;
                dust3.transform.localPosition = Vector3.zero;
                dust3.transform.localRotation = Quaternion.Euler(0, 270, 0);

                _dustPrefab.SetActive(false);
            }
            if (_gasPrefab == null)
            {
                _gasPrefab = new GameObject("Prefab_CO_Gas").DontDestroyOnLoad();

                var gas1 = SearchUtilities.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes/Effects_CO_GasTail").Instantiate();
                gas1.transform.parent = _gasPrefab.transform;
                gas1.transform.localPosition = Vector3.zero;
                gas1.transform.localRotation = Quaternion.Euler(0, 270, 0);

                var gas2 = SearchUtilities.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes/Effects_CO_GasTail (1)").Instantiate();
                gas2.transform.parent = _gasPrefab.transform;
                gas2.transform.localPosition = Vector3.zero;
                gas2.transform.localRotation = Quaternion.Euler(0, 270, 0);

                var gas3 = SearchUtilities.Find("Comet_Body/Sector_CO/Effects_CO/Effects_CO_TailMeshes/Effects_CO_GasTail (2)").Instantiate();
                gas3.transform.parent = _gasPrefab.transform;
                gas3.transform.localPosition = Vector3.zero;
                gas3.transform.localRotation = Quaternion.Euler(0, 270, 0);

                _gasPrefab.SetActive(false);
            }
        }

        public static void Make(GameObject planetGO, Sector sector, CometTailModule cometTailModule, PlanetConfig config, AstroObject ao)
        {
            var primaryBody = ao.GetPrimaryBody();

            if (!string.IsNullOrEmpty(config.Orbit.primaryBody)) primaryBody = AstroObjectLocator.GetAstroObject(config.Orbit.primaryBody);

            if (primaryBody == null)
            {
                NHLogger.LogError($"Comet {planetGO.name} does not orbit anything. That makes no sense");
                return;
            }

            if (string.IsNullOrEmpty(cometTailModule.primaryBody))
                cometTailModule.primaryBody = !string.IsNullOrEmpty(config.Orbit.primaryBody) ? config.Orbit.primaryBody
                    : (primaryBody._name == AstroObject.Name.CustomString ? primaryBody.GetCustomName() : primaryBody._name.ToString());

            var rootObj = new GameObject("CometRoot");
            rootObj.SetActive(false);
            rootObj.transform.parent = sector?.transform ?? planetGO.transform;
            rootObj.transform.localPosition = Vector3.zero;

            var controller = rootObj.AddComponent<CometTailController>();

            controller.size = (cometTailModule.innerRadius ?? config.Base.surfaceSize) / 110;

            if (cometTailModule.rotationOverride != null) controller.SetRotationOverride(cometTailModule.rotationOverride);

            Delay.FireOnNextUpdate(() =>
            {
                controller.SetPrimaryBody(
                    AstroObjectLocator.GetAstroObject(cometTailModule.primaryBody).transform,
                    primaryBody.GetAttachedOWRigidbody()
                );
            });

            controller.SetScaleCurve(cometTailModule.curve);

            var dustTail = Object.Instantiate(_dustPrefab, rootObj.transform).Rename("DustTail");
            dustTail.transform.localPosition = Vector3.zero;
            dustTail.transform.localRotation = Quaternion.Euler(90, 90, 0);
            dustTail.SetActive(true);
            controller.dustTail = dustTail;

            var gasTail = Object.Instantiate(_gasPrefab, rootObj.transform).Rename("GasTail");
            gasTail.transform.localPosition = Vector3.zero;
            gasTail.transform.localRotation = Quaternion.Euler(90, 90, 0);
            gasTail.SetActive(true);
            controller.gasTail = gasTail;

            if (cometTailModule.dustTint != null)
            {
                foreach (var dust in dustTail.GetComponentsInChildren<MeshRenderer>())
                {
                    var untintedDust = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/Effects_CO_DustTail_d.png");
                    dust.material.mainTexture = ImageUtilities.TintImage(untintedDust, cometTailModule.dustTint.ToColor());
                }
            }

            if (cometTailModule.gasTint != null)
            {
                foreach (var gas in gasTail.GetComponentsInChildren<MeshRenderer>())
                {
                    var untintedGas = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/Effects_CO_GasTail_d.png");
                    gas.material.mainTexture = untintedGas;
                    gas.material.color = cometTailModule.gasTint.ToColor();
                }
            }

            rootObj.SetActive(true);
        }
    }
}
