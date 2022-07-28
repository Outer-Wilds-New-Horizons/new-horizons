using System;
using System.Collections.Generic;
using NewHorizons.Utility;

namespace NewHorizons.External
{
    public static class NewHorizonsData
    {
        private static NewHorizonsSaveFile _saveFile;
        private static NewHorizonsProfile _activeProfile;
        private static string _activeProfileName;
        private static readonly string FileName = "save.json";

        public static void Load()
        {
            _activeProfileName = StandaloneProfileManager.SharedInstance?.currentProfile?.profileName;
            if (_activeProfileName == null)
            {
                Logger.LogError("Couldn't find active profile, are you on Gamepass?");
                _activeProfileName = "XboxGamepassDefaultProfile";
            }

            try
            {
                _saveFile = Main.Instance.ModHelper.Storage.Load<NewHorizonsSaveFile>(FileName);
                if (!_saveFile.Profiles.ContainsKey(_activeProfileName))
                    _saveFile.Profiles.Add(_activeProfileName, new NewHorizonsProfile());
                _activeProfile = _saveFile.Profiles[_activeProfileName];
                Logger.LogVerbose($"Loaded save data for {_activeProfileName}");
            }
            catch (Exception)
            {
                try
                {
                    Logger.LogVerbose($"Couldn't load save data from {FileName}, creating a new file");
                    _saveFile = new NewHorizonsSaveFile();
                    _saveFile.Profiles.Add(_activeProfileName, new NewHorizonsProfile());
                    _activeProfile = _saveFile.Profiles[_activeProfileName];
                    Main.Instance.ModHelper.Storage.Save(_saveFile, FileName);
                    Logger.LogVerbose($"Loaded save data for {_activeProfileName}");
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't create save data:\n{e}");
                }
            }
        }

        public static void Save()
        {
            if (_saveFile == null) return;
            Main.Instance.ModHelper.Storage.Save(_saveFile, FileName);
        }

        public static void Reset()
        {
            if (_saveFile == null || _activeProfile == null) Load();
            Logger.LogVerbose($"Resetting save data for {_activeProfileName}");
            _activeProfile = new NewHorizonsProfile();
            _saveFile.Profiles[_activeProfileName] = _activeProfile;

            Save();
        }

        private class NewHorizonsSaveFile
        {
            public NewHorizonsSaveFile()
            {
                Profiles = new Dictionary<string, NewHorizonsProfile>();
            }

            public Dictionary<string, NewHorizonsProfile> Profiles { get; }
        }

        private class NewHorizonsProfile
        {
            public NewHorizonsProfile()
            {
                KnownFrequencies = new List<string>();
                KnownSignals = new List<string>();
                NewlyRevealedFactIDs = new List<string>();
            }

            public List<string> KnownFrequencies { get; }
            public List<string> KnownSignals { get; }

            public List<string> NewlyRevealedFactIDs { get; }
        }

        #region Frequencies

        public static bool KnowsFrequency(string frequency)
        {
            if (_activeProfile == null) return true;
            return _activeProfile.KnownFrequencies.Contains(frequency);
        }

        public static void LearnFrequency(string frequency)
        {
            if (_activeProfile == null) return;
            if (!KnowsFrequency(frequency))
            {
                _activeProfile.KnownFrequencies.Add(frequency);
                Save();
            }
        }

        public static bool KnowsMultipleFrequencies()
        {
            return _activeProfile != null && _activeProfile.KnownFrequencies.Count > 0;
        }

        #endregion

        #region Signals

        public static bool KnowsSignal(string signal)
        {
            if (_activeProfile == null) return true;
            return _activeProfile.KnownSignals.Contains(signal);
        }

        public static void LearnSignal(string signal)
        {
            if (_activeProfile == null) return;
            if (!KnowsSignal(signal))
            {
                _activeProfile.KnownSignals.Add(signal);
                Save();
            }
        }

        #endregion

        #region Newly Revealed Facts

        public static void AddNewlyRevealedFactID(string id)
        {
            _activeProfile?.NewlyRevealedFactIDs.Add(id);
            Save();
        }

        public static List<string> GetNewlyRevealedFactIDs()
        {
            return _activeProfile?.NewlyRevealedFactIDs;
        }

        public static void ClearNewlyRevealedFactIDs()
        {
            _activeProfile?.NewlyRevealedFactIDs.Clear();
            Save();
        }

        #endregion
    }
}