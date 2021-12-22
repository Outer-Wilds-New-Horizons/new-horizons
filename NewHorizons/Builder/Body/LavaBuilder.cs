using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    static class LavaBuilder
    {
        public static void Make(GameObject body, Sector sector, OWRigidbody rb, float lavaSize) 
        {
            var moltenCore = new GameObject("MoltenCore");
            moltenCore.SetActive(false);
            moltenCore.transform.parent = body.transform;
            moltenCore.transform.localPosition = Vector3.zero;
            moltenCore.transform.localScale = Vector3.one * lavaSize;

            var lavaSphere = GameObject.Instantiate(GameObject.Find("VolcanicMoon_Body/MoltenCore_VM/LavaSphere"), moltenCore.transform);
            lavaSphere.transform.localScale = Vector3.one;
            var sectorCullGroup = lavaSphere.GetComponent<SectorCullGroup>();
            sectorCullGroup.SetSector(sector);

            var moltenCoreProxy = GameObject.Instantiate(GameObject.Find("VolcanicMoon_Body/MoltenCore_VM/MoltenCore_Proxy"), moltenCore.transform); ;
            moltenCoreProxy.name = "MoltenCore_Proxy";
        
            var proxyLavaSphere = moltenCoreProxy.transform.Find("LavaSphere (1)");
            proxyLavaSphere.transform.localScale = Vector3.one;
            proxyLavaSphere.name = "LavaSphere_Proxy";
           
            var sectorProxy = moltenCoreProxy.GetComponent<SectorProxy>();
            sectorProxy.SetValue("_renderers", new List<Renderer> { proxyLavaSphere.GetComponent<MeshRenderer>() });
            sectorProxy.SetSector(sector);
            
            var destructionVolume = GameObject.Instantiate(GameObject.Find("VolcanicMoon_Body/MoltenCore_VM/DestructionVolume"), moltenCore.transform);
            destructionVolume.GetComponent<SphereCollider>().radius = 1;
            
            moltenCore.SetActive(true);
            destructionVolume.SetActive(true);
        }
    }
}
