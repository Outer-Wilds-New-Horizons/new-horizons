using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class ShipWarpController : MonoBehaviour
    {
        private SingularityController _singularityController;
        private OWAudioSource _oneShotSource;

        public void Start()
        {
            GameObject singularityGO = GameObject.Instantiate(GameObject.Find("TowerTwin_Body/Sector_TowerTwin/Sector_Tower_HGT/Interactables_Tower_HGT/Interactables_Tower_TT/Prefab_NOM_WarpTransmitter (1)/BlackHole/BlackHoleSingularity"), GameObject.Find("Ship_Body").transform);
            singularityGO.transform.localPosition = new Vector3(0f, 0f, 5f);
            singularityGO.transform.localScale = Vector3.one * 10f;
            _singularityController = singularityGO.GetComponent<SingularityController>();

            _oneShotSource = singularityGO.AddComponent<OWAudioSource>();
        }

        public void WarpIn()
        {
            _oneShotSource.PlayOneShot(global::AudioType.VesselSingularityCollapse, 1f);
        }

        public void WarpOut()
        {
            _oneShotSource.PlayOneShot(global::AudioType.VesselSingularityCreate, 1f);
            _singularityController.Create();
        }
    }
}
