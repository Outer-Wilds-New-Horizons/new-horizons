using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class TornadoFix : SectoredMonoBehaviour
    {
        public TornadoController tornadoController;

        public new void Awake()
        {
            base.Awake();
            tornadoController = GetComponent<TornadoController>();
            tornadoController._tornadoRoot.SetActive(false);

            tornadoController._formationDuration = 1f;
            tornadoController._collapseDuration = 1f;
            if(_sector != null)
            {
                _sector.OnOccupantEnterSector += OnOccupantEnterSector;
                _sector.OnOccupantExitSector += OnOccupantExitSector;
            }
        }

        public new void SetSector(Sector sector)
        {
            if(_sector != null)
            {
                _sector.OnOccupantEnterSector -= OnOccupantEnterSector;
                _sector.OnOccupantExitSector -= OnOccupantExitSector;
            }

            base.SetSector(sector);
            _sector.OnOccupantEnterSector += OnOccupantEnterSector;
            _sector.OnOccupantExitSector += OnOccupantExitSector;
        }

        public new void OnDestroy()
        {
            base.OnDestroy();
            _sector.OnOccupantEnterSector -= OnOccupantEnterSector;
            _sector.OnOccupantExitSector -= OnOccupantExitSector;
        }

        public void OnOccupantEnterSector(SectorDetector _)
        {
            if(!tornadoController._tornadoRoot.activeInHierarchy) tornadoController.StartFormation();
        }

        public void OnOccupantExitSector(SectorDetector _)
        {
            if (!_sector.ContainsOccupant(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
            {
                tornadoController.StartCollapse();
            }
        }
    }
}
