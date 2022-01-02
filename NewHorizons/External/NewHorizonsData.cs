using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.External
{
    public static class NewHorizonsData
    {
        private static NewHorizonsSaveFile _saveFile;
        private static NewHorizonsProfile _activeProfile;
        private static string _activeProfileName;
        private static string _fileName = "save.json";

        public static void Load()
        {
            _activeProfileName = StandaloneProfileManager.SharedInstance.currentProfile.profileName;
            try
            {
                _saveFile = Main.Instance.ModHelper.Storage.Load<NewHorizonsSaveFile>(_fileName);
                if (!_saveFile.Profiles.ContainsKey(_activeProfileName)) _saveFile.Profiles.Add(_activeProfileName, new NewHorizonsProfile());
                _activeProfile = _saveFile.Profiles[_activeProfileName];
                Logger.Log($"Loaded save data for {_activeProfileName}");
            }
            catch(Exception)
            {
                try
                {
                    Logger.Log($"Couldn't load save data from {_fileName}, creating a new file");
                    _saveFile = new NewHorizonsSaveFile();
                    _saveFile.Profiles.Add(_activeProfileName, new NewHorizonsProfile());
                    _activeProfile = _saveFile.Profiles[_activeProfileName];
                    Main.Instance.ModHelper.Storage.Save(_saveFile, _fileName);
                    Logger.Log($"Loaded save data for {_activeProfileName}");
                }
                catch(Exception e)
                {
                    Logger.LogError($"Couldn't create save data {e.Message}, {e.StackTrace}");
                }
            }
        }

        public static void Save()
        {
            if (_saveFile == null) return;
            Main.Instance.ModHelper.Storage.Save(_saveFile, _fileName);
        }

        public static void Reset()
        {
            if (_saveFile == null || _activeProfile == null)
            {
                Load();
            }
            Logger.Log($"Reseting save data for {_activeProfileName}");
            _activeProfile = new NewHorizonsProfile();
            _saveFile.Profiles[_activeProfileName] = _activeProfile;

            Save();
        }

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

        public static bool KnowsMultipleFrequencies()
        {
            return (_activeProfile != null && _activeProfile.KnownFrequencies.Count > 0);
        }

        private class NewHorizonsSaveFile
        {
            public NewHorizonsSaveFile()
            {
                Profiles = new Dictionary<string, NewHorizonsProfile>();
            }

            public Dictionary<string, NewHorizonsProfile> Profiles { get; set; }
        }

        private class NewHorizonsProfile
        {
            public NewHorizonsProfile()
            {
                KnownFrequencies = new List<string>();
                KnownSignals = new List<string>();
            }

            public List<string> KnownFrequencies { get; set; }
            public List<string> KnownSignals { get; set; }
        }
    }
}
