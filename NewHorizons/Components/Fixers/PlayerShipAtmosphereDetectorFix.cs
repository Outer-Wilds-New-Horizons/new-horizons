using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Components.Fixers;

/// <summary>
/// Fixes a bug where spawning into the ship would not trigger the hatch entryway, so the player could drown if the ship flew into water
/// </summary>
internal class PlayerShipAtmosphereDetectorFix : MonoBehaviour
{
    private PlayerCameraFluidDetector _fluidDetector;
    private SimpleFluidVolume _shipAtmosphereVolume;

    public void Start()
    {
        _fluidDetector = Locator.GetPlayerCameraDetector().GetComponent<PlayerCameraFluidDetector>();
        _shipAtmosphereVolume = Locator.GetShipBody()?.transform?.Find("Volumes/ShipAtmosphereVolume")?.GetComponent<SimpleFluidVolume>();
        if (_shipAtmosphereVolume == null)
        {
            Destroy(this);
        }
    }

    public void Update()
    {
        if (PlayerState.IsInsideShip())
        {
            if (!_fluidDetector._activeVolumes.Contains(_shipAtmosphereVolume))
            {
                NHLogger.LogVerbose($"{nameof(PlayerShipAtmosphereDetectorFix)} had to add the ship atmosphere volume [{_shipAtmosphereVolume}] to the fluid detector");
                _fluidDetector.AddVolume(_shipAtmosphereVolume);
            }
            NHLogger.LogVerbose($"{nameof(PlayerShipAtmosphereDetectorFix)} applied its fix");
            Component.Destroy(this);
        }
    }
}
