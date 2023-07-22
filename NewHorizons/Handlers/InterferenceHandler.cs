using System.Collections.Generic;
using System.Linq;

namespace NewHorizons.Handlers
{
    using InterferenceVolume = Components.Volumes.InterferenceVolume;

    public static class InterferenceHandler
    {
        private static List<InterferenceVolume> _playerInterference;
        private static List<InterferenceVolume> _probeInterference;
        private static List<InterferenceVolume> _shipInterference;

        public static void Init()
        {
            _playerInterference = new List<InterferenceVolume>();
            _probeInterference = new List<InterferenceVolume>();
            _shipInterference = new List<InterferenceVolume>();
        }

        public static bool PlayerHasInterference()
        {
            if (_playerInterference == null) Init();
            return _playerInterference.Any(volume => volume != null);
        }
        public static bool ProbeHasInterference()
        {
            if (_probeInterference == null) Init();
            return _probeInterference.Any(volume => volume != null);
        }
        public static bool ShipHasInterference()
        {
            if (_shipInterference == null) Init();
            return _shipInterference.Any(volume => volume != null);
        }

        public static bool IsPlayerSameAsProbe()
        {
            if (_playerInterference == null || _probeInterference == null) Init();
            _playerInterference.RemoveAll(volume => volume == null);
            return _playerInterference.All(_probeInterference.Contains) && _playerInterference.Count == _probeInterference.Count;
        }

        public static bool IsPlayerSameAsShip()
        {
            if (_playerInterference == null || _shipInterference == null) Init();
            _playerInterference.RemoveAll(volume => volume == null);
            return _playerInterference.All(_shipInterference.Contains) && _playerInterference.Count == _shipInterference.Count;
        }

        public static void OnPlayerEnterInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            if (_playerInterference == null) Init();
            _playerInterference.SafeAdd(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnPlayerExitInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            if (_playerInterference == null) Init();
            _playerInterference.Remove(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnProbeEnterInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            if (_probeInterference == null) Init();
            _probeInterference.SafeAdd(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnProbeExitInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            if (_probeInterference == null) Init();
            _probeInterference.Remove(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }

        public static void OnShipEnterInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            if (_shipInterference == null) Init();
            _shipInterference.SafeAdd(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }
        public static void OnShipExitInterferenceVolume(InterferenceVolume interferenceVolume)
        {
            if (_shipInterference == null) Init();
            _shipInterference.Remove(interferenceVolume);
            GlobalMessenger.FireEvent("RefreshHUDVisibility");
        }
    }
}
