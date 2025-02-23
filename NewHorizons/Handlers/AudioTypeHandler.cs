using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace NewHorizons.Handlers
{
    public static class AudioTypeHandler
    {
        private static Dictionary<string, AudioType> _customAudioTypes;
        private static List<AudioLibrary.AudioEntry> _audioEntries;
        private static bool _postInitialized = false;

        public static void Init()
        {
            _customAudioTypes = new Dictionary<string, AudioType>();
            _audioEntries = new List<AudioLibrary.AudioEntry>();
            _postInitialized = false;

            Delay.RunWhen(() => Locator.GetAudioManager()?._libraryAsset != null && Locator.GetAudioManager()?._audioLibraryDict != null,
                PostInit
            );
        }

        private static void PostInit()
        {
            NHLogger.LogVerbose($"Adding all custom AudioTypes to the library");
            _postInitialized = true;
            ModifyAudioLibrary();
        }

        private static void ModifyAudioLibrary()
        {
            var library = Locator.GetAudioManager()._libraryAsset;
            var audioEntries = library.audioEntries; // store previous array
            library.audioEntries = library.audioEntries.Concat(_audioEntries).ToArray(); // concat custom entries
            Locator.GetAudioManager()._audioLibraryDict = library.BuildAudioEntryDictionary();
            library.audioEntries = audioEntries; // reset it back for next time we build
        }

        // Will return an existing audio type or create a new one for the given audio string
        public static AudioType GetAudioType(string audio, IModBehaviour mod)
        {
            try
            {
                if (audio.Contains(".wav") || audio.Contains(".mp3") || audio.Contains(".ogg"))
                {
                    return AddCustomAudioType(audio, mod);
                }
                else if (EnumUtils.TryParse<AudioType>(audio, out var type))
                {
                    return type;
                }
                else // Log if cannot parse
                {
                    NHLogger.LogError($"Couldn't load AudioType \"{audio}\"");
                    return AudioType.None;
                }

            }
            catch (Exception e)
            {
                NHLogger.LogError($"Couldn't load AudioType:\n{e}");
                return AudioType.None;
            }
        }

        // Create a custom audio type from relative file path and the mod
        public static AudioType AddCustomAudioType(string audioPath, IModBehaviour mod)
        {
            AudioType audioType;

            var id = mod.ModHelper.Manifest.UniqueName + "_" + audioPath;
            if (_customAudioTypes.TryGetValue(id, out audioType)) return audioType;

            var audioClip = AudioUtilities.LoadAudio(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, audioPath));

            if (audioClip == null)
            {
                NHLogger.LogError($"Couldn't create audioType for {audioPath}");
                return AudioType.None;
            }

            return AddCustomAudioType(id, new AudioClip[] { audioClip });
        }

        // Create a custom audio type from a set of audio clips. Needs a unique ID
        public static AudioType AddCustomAudioType(string id, AudioClip[] audioClips)
        {
            NHLogger.LogVerbose($"Registering new audio type [{id}]");

            var audioType = EnumUtilities.Create<AudioType>(id);

            _audioEntries.Add(new AudioLibrary.AudioEntry(audioType, audioClips));
            _customAudioTypes.Add(id, audioType);

            if (_postInitialized) ModifyAudioLibrary();

            return audioType;
        }
    }
}
