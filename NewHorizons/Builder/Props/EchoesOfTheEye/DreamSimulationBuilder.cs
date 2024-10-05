using NewHorizons.Components.Props;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Props.EchoesOfTheEye
{
    public static class DreamSimulationBuilder
    {
        private static Material gridMaterial;
        private static Material waterMaterial;

        private readonly static string[] EXCLUDED_OBJECT_NAMES =
        {
            "Prefab_IP_SIM_",
            "Props_IP_SIM_",
            "Effects_IP_SIM_",
        };

        private readonly static string[] EXCLUDED_SHADER_NAMES =
        {
            "Fog",
            "Simulation Bubble",
            "Foliage",
            "Flame",
        };

        public static void MakeDreamSimulationMeshes(GameObject go)
        {
            if (gridMaterial == null) gridMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Terrain_IP_DreamGrid_mat");
            if (waterMaterial == null) waterMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Terrain_IP_DreamGrid_mat");

            foreach (var mr in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (mr.GetType() != typeof(MeshRenderer)) continue;
                var mf = mr.GetComponent<MeshFilter>();
                if (mf == null) continue;
                if (!CheckMeshCreationHeuristic(mr.gameObject, mr.sharedMaterials)) continue;
                var simMesh = new GameObject("SimulationMesh").AddComponent<DreamSimulationMesh>();
                simMesh.Init(mr.transform, GetMeshMaterial(go, mr.sharedMaterials));
            }
        }

        private static Material GetMeshMaterial(GameObject go, Material[] materials)
        {
            if (materials.Any(m => m.name.Contains("Ocean_Stencil_mat"))) return waterMaterial;
            return gridMaterial;
        }

        private static bool CheckMeshCreationHeuristic(GameObject go, Material[] materials)
        {
            if (go.layer == Layer.DreamSimulation) return false;
            var mr = go.GetComponent<MeshRenderer>();
            if (EXCLUDED_SHADER_NAMES.Any(name => materials.Any(mat => mat.shader.name.Contains(name)))) return false;
            if (go.transform.parent)
            {
                foreach (Transform c in go.transform.parent)
                {
                    if (c && c.gameObject.layer == Layer.DreamSimulation) return false;
                }
            }
            if (go.transform.parent.parent)
            {
                foreach (Transform c in go.transform.parent.parent)
                {
                    if (c && c.gameObject.layer == Layer.DreamSimulation) return false;
                }
            }
            var t = go.transform;
            while (t != null)
            {
                if (EXCLUDED_OBJECT_NAMES.Any(t.name.Contains)) return false;
                t = t.parent;
            }
            return true;
        }
    }
}
