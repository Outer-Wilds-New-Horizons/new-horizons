using UnityEngine;

namespace NewHorizons.Components
{
    [RequireComponent(typeof(TessellatedSphereRenderer))]
    public class CloakedTessSphereSectorToggle : SectoredMonoBehaviour
    {
        protected TessellatedSphereRenderer _renderer;
        protected bool _inMapView;
        protected bool _inCloakField;

        public override void Awake()
        {
            _renderer = GetComponent<TessellatedSphereRenderer>();
            GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
            GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
        }

        public override void OnDestroy()
        {
            GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
            GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
        }

        public override void OnChangeSector(Sector oldSector, Sector newSector) => OnSectorOccupantsUpdated();

        public override void OnSectorOccupantsUpdated()
        {
            if (_inMapView || !_inCloakField)
                return;
            if (_sector != null)
            {
                if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe) && !_renderer.enabled)
                {
                    _renderer.enabled = true;
                }
                else
                {
                    if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe) || !_renderer.enabled)
                        return;
                    _renderer.enabled = false;
                }
            }
            else
                _renderer.enabled = true;
        }

        public virtual void OnEnterMapView()
        {
            _inMapView = true;
            if (_renderer.enabled)
                _renderer.enabled = false;
        }

        public virtual void OnExitMapView()
        {
            _inMapView = false;
            OnSectorOccupantsUpdated();
        }

        public virtual void OnEnterCloakField()
        {
            _inCloakField = true;
            OnSectorOccupantsUpdated();
        }

        public virtual void OnExitCloakField()
        {
            _inCloakField = false;
            if (_renderer.enabled)
                _renderer.enabled = false;
        }
    }
}
