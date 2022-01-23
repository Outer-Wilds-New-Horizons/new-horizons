using NewHorizons.Components;
using NewHorizons.External.VariableSize;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    public static class FunnelBuilder
    {
        private enum FunnelType
        {
            SAND,
            WATER,
            LAVA,
            STAR
        }

        public static void Make(GameObject go, ConstantForceDetector detector, OWRigidbody rigidbody, FunnelModule module)
        {
            var funnelType = FunnelType.SAND;
            if (module.Type.ToUpper().Equals("WATER")) funnelType = FunnelType.WATER;
            else if (module.Type.ToUpper().Equals("LAVA")) funnelType = FunnelType.LAVA;
            else if (module.Type.ToUpper().Equals("STAR")) funnelType = FunnelType.STAR;

            var funnelGO = new GameObject($"{go.name.Replace("_Body", "")}Funnel_Body");
            funnelGO.SetActive(false);
            funnelGO.transform.parent = go.transform;

            var owrb = funnelGO.AddComponent<OWRigidbody>();
            var alignment = funnelGO.AddComponent<AlignWithTargetBody>();

            funnelGO.AddComponent<InitialMotion>();

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
            scaleRoot.transform.rotation = Quaternion.Euler(90, 0, 0);
            scaleRoot.transform.localPosition = new Vector3(0, 30, 0);
            scaleRoot.transform.localScale = new Vector3(1, 1, 1.075f);

            var proxyGO = GameObject.Instantiate(GameObject.Find("SandFunnel_Body/ScaleRoot/Proxy_SandFunnel"), scaleRoot.transform);
            proxyGO.name = "Proxy_Funnel";

            var geoGO = GameObject.Instantiate(GameObject.Find("SandFunnel_Body/ScaleRoot/Geo_SandFunnel"), scaleRoot.transform);
            geoGO.name = "Geo_Funnel";

            var volumesGO = GameObject.Instantiate(GameObject.Find("SandFunnel_Body/ScaleRoot/Volumes_SandFunnel"), scaleRoot.transform);
            volumesGO.name = "Volumes_Funnel";
            var sfv = volumesGO.GetComponentInChildren<SimpleFluidVolume>();
            var fluidVolume = sfv.gameObject;
            switch (funnelType)
            {
                case FunnelType.SAND:
                    sfv._fluidType = FluidVolume.Type.SAND;
                    break;
                case FunnelType.WATER:
                    sfv._fluidType = FluidVolume.Type.WATER;

                    GameObject.Destroy(geoGO.transform.Find("Effects_HT_SandColumn/SandColumn_Interior").gameObject);

                    var waterMaterial = GameObject.Find("TimberHearth_Body/Sector_TH/Geometry_TH/Terrain_TH_Water_v3/Village_Upper_Water/Village_Upper_Water_Geo").GetComponent<MeshRenderer>().material;
                    proxyGO.GetComponentInChildren<MeshRenderer>().material = waterMaterial;
                    geoGO.GetComponentInChildren<MeshRenderer>().material = waterMaterial;

                    break;
                case FunnelType.LAVA:
                    sfv._fluidType = FluidVolume.Type.PLASMA;

                    GameObject.Destroy(geoGO.transform.Find("Effects_HT_SandColumn/SandColumn_Interior").gameObject);

                    var lavaMaterial = GameObject.Find("VolcanicMoon_Body/MoltenCore_VM/LavaSphere").GetComponent<MeshRenderer>().material;
                    proxyGO.GetComponentInChildren<MeshRenderer>().material = lavaMaterial;
                    geoGO.GetComponentInChildren<MeshRenderer>().material = lavaMaterial;

                    AddDestructionVolumes(fluidVolume);

                    break;
                case FunnelType.STAR:
                    sfv._fluidType = FluidVolume.Type.PLASMA;

                    GameObject.Destroy(geoGO.transform.Find("Effects_HT_SandColumn/SandColumn_Interior").gameObject);

                    var starMaterial = GameObject.Find("VolcanicMoon_Body/MoltenCore_VM/LavaSphere").GetComponent<MeshRenderer>().material;
                    proxyGO.GetComponentInChildren<MeshRenderer>().material = starMaterial;
                    geoGO.GetComponentInChildren<MeshRenderer>().material = starMaterial;

                    AddDestructionVolumes(fluidVolume);

                    break;
            }

            var sector = go.GetComponent<AstroObject>().GetPrimaryBody().GetRootSector();
            proxyGO.GetComponent<SectorProxy>().SetSector(sector);
            geoGO.GetComponent<SectorCullGroup>().SetSector(sector);
            volumesGO.GetComponent<SectorCollisionGroup>().SetSector(sector);

            funnelGO.transform.localPosition = Vector3.zero;

            var funnelSizeController = funnelGO.AddComponent<FunnelSizeController>();

            if(module.Curve != null)
            {
                var curve = new AnimationCurve();
                foreach (var pair in module.Curve)
                {
                    curve.AddKey(new Keyframe(pair.Time, pair.Value));
                }
                funnelSizeController.scaleCurve = curve;
            }

            // Finish up next tick
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => PostMake(funnelGO, alignment, funnelSizeController, module));
        }

        private static void PostMake(GameObject funnelGO, AlignWithTargetBody alignment, FunnelSizeController funnelSizeController, FunnelModule module)
        {
            var target = AstroObjectLocator.GetAstroObject(module.Target)?.GetAttachedOWRigidbody();
            if(target == null)
            {
                Logger.LogWarning($"Couldn't find the target for the funnel {funnelGO.name}");
                return;
            }

            alignment.SetTargetBody(target);

            funnelSizeController.target = target.gameObject.transform;

            funnelGO.SetActive(true);

            // This has to happen last idk
            alignment.SetUsePhysicsToRotate(true);
        }

        private static void AddDestructionVolumes(GameObject go)
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
                destructionVolume._deathType = DeathType.Lava;
            }
        }
    }
}
