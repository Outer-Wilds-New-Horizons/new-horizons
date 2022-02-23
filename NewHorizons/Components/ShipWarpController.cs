using NewHorizons.Builder.General;
using PacificEngine.OW_CommonResources.Game.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Components
{
    public class ShipWarpController : MonoBehaviour
    {
        private SingularityController _blackhole;
        private SingularityController _whitehole;
        private OWAudioSource _oneShotSource;

        private bool _isWarpingIn;
        private bool _wearingSuit;
        private bool _waitingToBeSeated;
        private bool _eyesOpen = false;

        private float _impactDeathSpeed;

        private const float size = 14f;

        public void Start()
        {
            MakeBlackHole();
            MakeWhiteHole();

            _isWarpingIn = false;

            _oneShotSource = base.gameObject.AddComponent<OWAudioSource>();

            GlobalMessenger.AddListener("FinishOpenEyes", new Callback(OnFinishOpenEyes));
        }

        public void OnDestroy()
        {
            GlobalMessenger.RemoveListener("FinishOpenEyes", new Callback(OnFinishOpenEyes));
        }

        private void MakeBlackHole()
        {
            var blackHoleShader = GameObject.Find("TowerTwin_Body/Sector_TowerTwin/Sector_Tower_HGT/Interactables_Tower_HGT/Interactables_Tower_TT/Prefab_NOM_WarpTransmitter (1)/BlackHole/BlackHoleSingularity").GetComponent<MeshRenderer>().material.shader;

            var blackHoleRender = new GameObject("BlackHoleRender");
            blackHoleRender.transform.parent = base.transform;
            blackHoleRender.transform.localPosition = new Vector3(0, 1, 0);
            blackHoleRender.transform.localScale = Vector3.one * size;

            var meshFilter = blackHoleRender.AddComponent<MeshFilter>();
            meshFilter.mesh = GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer").GetComponent<MeshFilter>().mesh;

            var meshRenderer = blackHoleRender.AddComponent<MeshRenderer>();
            if (blackHoleShader == null) blackHoleShader = GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer").GetComponent<MeshRenderer>().sharedMaterial.shader;
            meshRenderer.material = new Material(blackHoleShader);
            meshRenderer.material.SetFloat("_Radius", size * 0.4f);
            meshRenderer.material.SetFloat("_MaxDistortRadius", size * 0.95f);
            meshRenderer.material.SetFloat("_MassScale", 1);
            meshRenderer.material.SetFloat("_DistortFadeDist", size * 0.55f);

            _blackhole = blackHoleRender.AddComponent<SingularityController>();
            blackHoleRender.SetActive(true);
        }

        private void MakeWhiteHole()
        {
            var whiteHoleShader = GameObject.Find("TowerTwin_Body/Sector_TowerTwin/Sector_Tower_HGT/Interactables_Tower_HGT/Interactables_Tower_CT/Prefab_NOM_WarpTransmitter/WhiteHole/WhiteHoleSingularity").GetComponent<MeshRenderer>().material.shader;

            var whiteHoleRenderer = new GameObject("WhiteHoleRenderer");
            whiteHoleRenderer.transform.parent = base.transform;
            whiteHoleRenderer.transform.localPosition = new Vector3(0, 1, 0);
            whiteHoleRenderer.transform.localScale = Vector3.one * size * 2.8f;

            var meshFilter = whiteHoleRenderer.AddComponent<MeshFilter>();
            meshFilter.mesh = GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshFilter>().mesh;

            var meshRenderer = whiteHoleRenderer.AddComponent<MeshRenderer>();
            if (whiteHoleShader == null) whiteHoleShader = GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshRenderer>().sharedMaterial.shader;
            meshRenderer.material = new Material(whiteHoleShader);
            meshRenderer.sharedMaterial.SetFloat("_Radius", size * 0.4f);
            meshRenderer.sharedMaterial.SetFloat("_DistortFadeDist", size);
            meshRenderer.sharedMaterial.SetFloat("_MaxDistortRadius", size * 2.8f);
            meshRenderer.sharedMaterial.SetColor("_Color", new Color(1.88f, 1.88f, 1.88f, 1f));

            _whitehole = whiteHoleRenderer.AddComponent<SingularityController>();
            whiteHoleRenderer.SetActive(true);
        }

        public void WarpIn(bool wearingSuit)
        {
            Logger.Log("Starting warp-in");
            // Trying really hard to stop the player from dying while warping in
            _impactDeathSpeed = Locator.GetDeathManager()._impactDeathSpeed;
            Locator.GetDeathManager()._impactDeathSpeed = Mathf.Infinity;
            Locator.GetDeathManager()._invincible = true;

            _isWarpingIn = true;
            _wearingSuit = wearingSuit;
            _whitehole.Create();
        }

        public void WarpOut()
        {
            Logger.Log("Starting warp-out");
            _oneShotSource.PlayOneShot(global::AudioType.VesselSingularityCreate, 1f);
            _blackhole.Create();
        }

        public void Update()
        {
            if(_isWarpingIn && LateInitializerManager.isDoneInitializing)
            {
                Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => StartWarpInEffect(), 1);
                _isWarpingIn = false;
            }

            if (_waitingToBeSeated)
            {
                if (Player.getResources()._currentHealth < 100f)
                {
                    Logger.Log("Player died in a warp drive accident, reviving them");
                    // Means the player was killed meaning they weren't teleported in
                    Player.getResources()._currentHealth = 100f;
                    if (!PlayerState.AtFlightConsole()) TeleportToShip();
                }
            }

            // Idk whats making this work but now it works and idc
            if (_waitingToBeSeated && PlayerState.IsInsideShip() && _eyesOpen)
            {
                Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => FinishWarpIn(), 1);
                _waitingToBeSeated = false;
            }
        }

        private void OnFinishOpenEyes()
        {
            _eyesOpen = true;
        }

        private void StartWarpInEffect()
        {
            Logger.Log("Starting warp-in effect");
            _oneShotSource.PlayOneShot(global::AudioType.VesselSingularityCollapse, 1f);
            Locator.GetDeathManager()._invincible = true;
            if (Main.Instance.CurrentStarSystem.Equals("SolarSystem")) TeleportToShip();
            _whitehole.Create();
            _waitingToBeSeated = true;
            if (_wearingSuit && !Locator.GetPlayerController()._isWearingSuit)
            {
                SpawnPointBuilder.SuitUp();
            }
        }

        private void TeleportToShip()
        {
            var playerSpawner = GameObject.FindObjectOfType<PlayerSpawner>();
            playerSpawner.DebugWarp(playerSpawner.GetSpawnPoint(SpawnLocation.Ship));
        }

        public void FinishWarpIn()
        {
            Logger.Log("Finishing warp");
            Locator.GetShipBody().GetComponentInChildren<ShipCockpitController>().OnPressInteract();
            _waitingToBeSeated = false;
            Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => _whitehole.Collapse(), 30);

            Locator.GetDeathManager()._impactDeathSpeed = _impactDeathSpeed;
            Player.getResources()._currentHealth = 100f;
            Locator.GetDeathManager()._invincible = false;

            // For some reason warping into the ship makes you suffocate while in the ship
            if(_wearingSuit) Player.getResources().OnSuitUp();
            var o2Volume = Locator.GetShipBody().GetComponent<OxygenVolume>();
            var atmoVolume = GameObject.Find("Ship_Body/Volumes/ShipAtmosphereVolume").GetComponent<SimpleFluidVolume>();

            Player.getResources()._cameraFluidDetector.AddVolume(atmoVolume);
            Player.getResources()._cameraFluidDetector.OnVolumeAdded(atmoVolume);
            Player.getResources()._cameraFluidDetector.OnVolumeActivated(atmoVolume);

            GlobalMessenger.FireEvent("EnterShip");
            PlayerState.OnEnterShip();
        }
    }
}
