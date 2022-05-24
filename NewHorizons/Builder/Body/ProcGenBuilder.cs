using NewHorizons.Builder.Body.Geometry;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.Body
{
    public static class ProcGenBuilder
    {
        private static Material quantumMaterial;
        private static Material iceMaterial;

        public static void Make(GameObject planetGO, Sector sector, ProcGenModule module)
        {
            if (quantumMaterial == null) quantumMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Rock_QM_EyeRock_mat");
            if (iceMaterial == null) iceMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Rock_BH_IceSpike_mat");


            GameObject icosphere = new GameObject("Icosphere");
            icosphere.transform.parent = sector?.transform ?? planetGO.transform;
            icosphere.transform.rotation = Quaternion.Euler(90, 0, 0);
            icosphere.transform.position = planetGO.transform.position;

            Mesh mesh = Icosphere.Build(4, module.scale, module.scale * 1.2f);

            icosphere.AddComponent<MeshFilter>();
            icosphere.GetComponent<MeshFilter>().mesh = mesh;

            var cubeSphereMR = icosphere.AddComponent<MeshRenderer>();
            cubeSphereMR.material = new Material(Shader.Find("Standard"));
            cubeSphereMR.material.color = module.color != null ? module.color.ToColor() : Color.white;

            var cubeSphereMC = icosphere.AddComponent<MeshCollider>();
            cubeSphereMC.sharedMesh = mesh;
            icosphere.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(90, 0, 0));

            icosphere.AddComponent<ProxyShadowCaster>();
        }
    }
}
