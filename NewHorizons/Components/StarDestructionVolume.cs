using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class StarDestructionVolume : DestructionVolume
    {
        public override void Vanish(OWRigidbody bodyToVanish, RelativeLocationData entryLocation)
        {
            // Don't vanish other stars
            if (bodyToVanish.GetComponent<StarController>() != null) return;

            base.Vanish(bodyToVanish, entryLocation);
        }

        public new void OnTriggerEnter(Collider hitCollider)
        {
            if (hitCollider.attachedRigidbody == null) return;

            if (hitCollider.attachedRigidbody.CompareTag("Player"))
            {
                _playerBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
                _playerLocation = new RelativeLocationData(_playerBody, transform);
            }
            else if (hitCollider.attachedRigidbody.CompareTag("Ship"))
            {
                _shipBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
                _shipLocation = new RelativeLocationData(_shipBody, transform);
            }
            else if (hitCollider.attachedRigidbody.CompareTag("ShipCockpit"))
            {
                _shipCockpitBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
                _shipCockpitLocation = new RelativeLocationData(_shipCockpitBody, transform);
            }
            else if (hitCollider.attachedRigidbody.CompareTag("Probe"))
            {
                _probeBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
                _probeLocation = new RelativeLocationData(_probeBody, transform);
            }
            else if (hitCollider.attachedRigidbody.CompareTag("ModelRocketShipBody"))
            {
                _modelShipBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
                _modelShipLocation = new RelativeLocationData(_modelShipBody, transform);
            }
            else if (hitCollider.attachedRigidbody.CompareTag("NomaiShuttleBody"))
            {
                _nomaiShuttleBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
                _nomaiShuttleLocation = new RelativeLocationData(_nomaiShuttleBody, transform);
            }
            else
            {
                if (_onlyAffectsPlayerAndShip) return;
                OWRigidbody owrb = hitCollider.attachedRigidbody.GetComponent<OWRigidbody>();
                if (owrb != null)
                {
                    // Don't shrink/vanish other stars
                    if (owrb.GetComponent<StarController>() != null) return;

                    if (_shrinkBodies)
                        Shrink(owrb);
                    else
                        Vanish(owrb, new RelativeLocationData(owrb, transform));
                }
            }
            if (_vanishEffectPool != null) _vanishEffectPool.Instantiate(transform, hitCollider.transform.position, Quaternion.FromToRotation(transform.forward, hitCollider.transform.position - transform.position) * transform.rotation);
        }
    }
}
