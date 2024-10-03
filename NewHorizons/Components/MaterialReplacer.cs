using NewHorizons.Utility;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components
{
    /// <summary>
    /// Used by vessel asset bundle to change materials to the in-game ones.
    /// </summary>
    [UsedInUnityProject]
    public class MaterialReplacer : MonoBehaviour
    {
        public string[] materialNames;

        public void Start()
        {
            Renderer renderer = GetComponent<Renderer>();
            NomaiNodeController nnc = GetComponent<NomaiNodeController>();
            if (renderer != null)
            {
                var materials = materialNames.Select(name => SearchUtilities.FindResourceOfTypeAndName<Material>(name)).ToArray();
                if (renderer is ParticleSystemRenderer psr)
                    psr.materials = materials;
                else
                    renderer.sharedMaterials = materials;
            }
            else if (nnc != null)
            {
                var materials = materialNames.Select(name => SearchUtilities.FindResourceOfTypeAndName<Material>(name)).ToArray();
                nnc._inactiveMaterial = materials[0];
                nnc._activeMaterial = materials[1];
            }

            NomaiLamp nl = GetComponentInParent<NomaiLamp>();
            if (nl != null)
            {
                nl.enabled = true;
                nl.Awake();
            }
        }
    }
}
