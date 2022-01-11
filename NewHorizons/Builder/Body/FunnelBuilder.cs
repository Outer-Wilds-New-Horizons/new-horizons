using NewHorizons.External.VariableSize;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        public static void Make(GameObject go, ConstantForceDetector detector, FunnelModule module)
        {
            var funnelType = FunnelType.SAND;
            if (module.Type.ToUpper().Equals("WATER")) funnelType = FunnelType.WATER;
            else if (module.Type.ToUpper().Equals("LAVA")) funnelType = FunnelType.LAVA;
            else if (module.Type.ToUpper().Equals("STAR")) funnelType = FunnelType.STAR;

            var funnelGO = new GameObject($"{go.name.Replace("_Body", "")}Funnel_Body");
            funnelGO.SetActive(false);
            
            var owrb = funnelGO.AddComponent<OWRigidbody>();
            var alignment = funnelGO.AddComponent<AlignWithTargetBody>();
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => alignment.SetTargetBody(AstroObjectLocator.GetAstroObject(module.Target)?.GetAttachedOWRigidbody()));
            funnelGO.AddComponent<InitialMotion>();
            funnelGO.AddComponent<SandFunnelController>();
            funnelGO.AddComponent<CenterOfTheUniverseOffsetApplier>();
            funnelGO.AddComponent<KinematicRigidbody>();

            var detectorGO = new GameObject("Detector_Funnel");
            detectorGO.transform.parent = funnelGO.transform;
            var funnelDetector = detectorGO.AddComponent<ConstantForceDetector>();
            funnelDetector._inheritDetector = detector; 

            detectorGO.AddComponent<ForceApplier>();

            var scaleRoot = new GameObject("ScaleRoot");
            scaleRoot.transform.parent = go.transform;

            var proxyGO = GameObject.Instantiate(GameObject.Find("SandFunnel_Body/ScaleRoot/Proxy_SandFunnel"), scaleRoot.transform);
            proxyGO.name = "Proxy_Funnel";

            var geoGO = GameObject.Instantiate(GameObject.Find("SandFunnel_Body/ScaleRoot/Geo_SandFunnel"), scaleRoot.transform);
            geoGO.name = "Geo_Funnel";

            var volumesGO = GameObject.Instantiate(GameObject.Find("SandFunnel_Body/ScaleRoot/Volumes_SandFunnel"), scaleRoot.transform);
            volumesGO.name = "Volumes_Funnel";
            var sfv = volumesGO.GetComponent<SimpleFluidVolume>();
            switch(funnelType)
            {
                case FunnelType.SAND:
                    sfv._fluidType = FluidVolume.Type.SAND;
                    break;
                case FunnelType.WATER:
                    sfv._fluidType = FluidVolume.Type.WATER;
                    break;
                case FunnelType.LAVA:
                    sfv._fluidType = FluidVolume.Type.PLASMA;
                    break;
                case FunnelType.STAR:
                    sfv._fluidType = FluidVolume.Type.PLASMA;
                    break;
            }
            
        }
    }
}
