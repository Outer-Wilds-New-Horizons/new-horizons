using System.Runtime.Serialization;
using NewHorizons.Components;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.External.Modules.VariableSize;

namespace NewHorizons.Builder.Body
{
    public static class FunnelBuilder
    {
        private static readonly int FogColor = Shader.PropertyToID("_FogColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int HeightScale = Shader.PropertyToID("_HeightScale");

        public static void Make(GameObject planetGO, ConstantForceDetector detector, OWRigidbody rigidbody, FunnelModule module)
        {
            var funnelType = module.type;

            var funnelGO = new GameObject($"{planetGO.name.Replace("_Body", "")}Funnel_Body");
            funnelGO.SetActive(false);
            funnelGO.transform.parent = planetGO.transform;

            funnelGO.AddComponent<OWRigidbody>();

            var matchMotion = funnelGO.AddComponent<MatchInitialMotion>();
            matchMotion.SetBodyToMatch(rigidbody);

            funnelGO.AddComponent<CenterOfTheUniverseOffsetApplier>();
            funnelGO.AddComponent<KinematicRigidbody>();

            var detectorGO = new GameObject("Detector_Funnel");
            detectorGO.transform.parent = funnelGO.transform;
            var funnelDetector = detectorGO.AddComponent<ConstantForceDetector>();
            funnelDetector._inheritDetector = detector;
            funnelDetector._detectableFields = new ForceVolume[0];

            detectorGO.AddComponent<ForceApplier>();

            var scaleRoot = new GameObject("ScaleRoot");
            scaleRoot.transform.parent = funnelGO.transform;
            scaleRoot.transform.rotation = Quaternion.identity;
            scaleRoot.transform.localPosition = Vector3.zero;
            scaleRoot.transform.localScale = new Vector3(1, 1, 1);

            var proxyGO = GameObject.Instantiate(SearchUtilities.Find("SandFunnel_Body/ScaleRoot/Proxy_SandFunnel"), scaleRoot.transform);
            proxyGO.name = "Proxy_Funnel";

            var geoGO = GameObject.Instantiate(SearchUtilities.Find("SandFunnel_Body/ScaleRoot/Geo_SandFunnel"), scaleRoot.transform);
            geoGO.name = "Geo_Funnel";

            var volumesGO = GameObject.Instantiate(SearchUtilities.Find("SandFunnel_Body/ScaleRoot/Volumes_SandFunnel"), scaleRoot.transform);
            volumesGO.name = "Volumes_Funnel";
            var sfv = volumesGO.GetComponentInChildren<SimpleFluidVolume>();
            var fluidVolume = sfv.gameObject;
            switch (funnelType)
            {
                case FunnelType.Sand:
                    sfv._fluidType = FluidVolume.Type.SAND;
                    break;
                case FunnelType.Water:
                    sfv._fluidType = FluidVolume.Type.WATER;

                    GameObject.Destroy(geoGO.transform.Find("Effects_HT_SandColumn/SandColumn_Interior").gameObject);

                    var waterMaterials = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Geometry_TH/Terrain_TH_Water_v3/Village_Upper_Water/Village_Upper_Water_Geo").GetComponent<MeshRenderer>().materials;
                    var materials = new Material[waterMaterials.Length];
                    for (int i = 0; i < waterMaterials.Length; i++)
                    {
                        materials[i] = new Material(waterMaterials[i]);
                        if (module.tint != null)
                        {
                            materials[i].SetColor(FogColor, module.tint.ToColor());
                        }
                    }

                    // Proxy
                    var proxyExterior = proxyGO.transform.Find("SandColumn_Exterior (1)");
                    proxyExterior.name = "WaterColumn_Exterior";
                    var proxyExteriorMR = proxyExterior.GetComponent<MeshRenderer>();
                    proxyExteriorMR.material = materials[0];
                    proxyExteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    /*
                    var proxyInterior = GameObject.Instantiate(proxyExterior, proxyGO.transform);
                    proxyInterior.name = "WaterColumn_Interior";
                    var proxyInteriorMR = proxyInterior.GetComponent<MeshRenderer>();
                    proxyInteriorMR.material = materials[1];
                    proxyInteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    */

                    // Geometry
                    var geoExterior = geoGO.transform.Find("Effects_HT_SandColumn/SandColumn_Exterior");
                    geoExterior.name = "WaterColumn_Exterior";
                    var geoExteriorMR = geoExterior.GetComponent<MeshRenderer>();
                    geoExteriorMR.material = materials[0];
                    geoExteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    /*
                    var geoInterior = GameObject.Instantiate(geoExterior, geoGO.transform);
                    geoInterior.name = "WaterColumn_Interior";
                    var geoInteriorMR = geoInterior.GetComponent<MeshRenderer>();
                    geoInteriorMR.material = materials[1];
                    geoInteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    */

                    break;
                case FunnelType.Lava:
                case FunnelType.Star:
                    sfv._fluidType = FluidVolume.Type.PLASMA;

                    GameObject.Destroy(geoGO.transform.Find("Effects_HT_SandColumn/SandColumn_Interior").gameObject);

                    var lavaMaterial = new Material(SearchUtilities.Find("VolcanicMoon_Body/MoltenCore_VM/LavaSphere").GetComponent<MeshRenderer>().material);
                    lavaMaterial.mainTextureOffset = new Vector2(0.1f, 0.2f);
                    lavaMaterial.mainTextureScale = new Vector2(1f, 3f);

                    if (module.tint != null)
                    {
                        lavaMaterial.SetColor(EmissionColor, module.tint.ToColor());
                    }

                    proxyGO.GetComponentInChildren<MeshRenderer>().material = lavaMaterial;
                    geoGO.GetComponentInChildren<MeshRenderer>().material = lavaMaterial;

                    if (funnelType == FunnelType.Lava)
                    {
                        lavaMaterial.SetFloat(HeightScale, 0);
                        AddDestructionVolumes(fluidVolume, DeathType.Lava);
                    }
                    else if (funnelType == FunnelType.Star)
                    {
                        lavaMaterial.renderQueue = 2999;
                        lavaMaterial.SetFloat(HeightScale, 100000);
                        AddDestructionVolumes(fluidVolume, DeathType.Energy);
                    }

                    break;
            }

            var sector = planetGO.GetComponent<AstroObject>().GetPrimaryBody().GetRootSector();
            proxyGO.GetComponent<SectorProxy>().SetSector(sector);
            geoGO.GetComponent<SectorCullGroup>().SetSector(sector);
            volumesGO.GetComponent<SectorCollisionGroup>().SetSector(sector);

            funnelGO.transform.localPosition = Vector3.zero;

            var funnelSizeController = funnelGO.AddComponent<FunnelController>();

            if (module.curve != null)
            {
                var curve = new AnimationCurve();
                foreach (var pair in module.curve)
                {
                    curve.AddKey(new Keyframe(pair.time, pair.value));
                }
                funnelSizeController.scaleCurve = curve;
            }
            funnelSizeController.anchor = planetGO.transform;

            // Finish up next tick
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => PostMake(funnelGO, funnelSizeController, module));
        }

        private static void PostMake(GameObject funnelGO, FunnelController funnelSizeController, FunnelModule module)
        {
            var targetAO = AstroObjectLocator.GetAstroObject(module.target);
            var target = targetAO?.GetAttachedOWRigidbody();
            if (target == null)
            {
                if (targetAO != null) Logger.LogError($"Found funnel target ({targetAO.name}) but couldn't find rigidbody for the funnel {funnelGO.name}");
                else Logger.LogError($"Couldn't find the target ({module.target}) for the funnel {funnelGO.name}");
                return;
            }

            funnelSizeController.target = target.gameObject.transform;

            funnelGO.SetActive(true);
        }

        private static void AddDestructionVolumes(GameObject go, DeathType deathType)
        {
            // Gotta put destruction volumes on the children reeeeeeeeee
            foreach (Transform child in go.transform)
            {
                var capsuleShape = child.GetComponent<CapsuleShape>();

                var capsuleCollider = child.gameObject.AddComponent<CapsuleCollider>();
                capsuleCollider.radius = capsuleShape.radius;
                capsuleCollider.height = capsuleShape.height;
                capsuleCollider.isTrigger = true;

                child.gameObject.AddComponent<OWCapsuleCollider>();

                var destructionVolume = child.gameObject.AddComponent<DestructionVolume>();
                destructionVolume._deathType = deathType;
                // Only stars should destroy planets
                destructionVolume._onlyAffectsPlayerAndShip = deathType != DeathType.Energy;
            }
        }
    }
}
