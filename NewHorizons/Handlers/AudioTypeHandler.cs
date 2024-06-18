using NAudio.Mixer;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewHorizons.Handlers
{
    public static class AudioTypeHandler
    {
        private static Dictionary<string, AudioType> _customAudioTypes;

        private static AudioLibrary.AudioEntry[] _defaultLibraryEntries;
        private static AudioLibrary _library;

        private static bool _isSetUp;

        public static void Init()
        {
            _customAudioTypes = new Dictionary<string, AudioType>();

            Delay.RunWhen(
                () => Locator.GetAudioManager()?._libraryAsset != null,
                PostInit
            );

            if (!_isSetUp)
            {
                _isSetUp = true;
                SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
            }
        }

        private static void SceneManager_sceneUnloaded(Scene arg0)
        {
            if (_library != null)
            {
                _library.audioEntries = _defaultLibraryEntries; // reset it back for next time we build
                _library = null;
            }
        }

        private static void PostInit()
        {
            NHLogger.LogVerbose($"Adding all custom AudioTypes to the library");

            _library = Locator.GetAudioManager()._libraryAsset;
            _defaultLibraryEntries = _library.audioEntries; // store previous array
        }

        // Will return an existing audio type or create a new one for the given audio string
        public static AudioUtilities.AsyncAudioLoader AsyncSetAudioType(string audio, IModBehaviour mod, Action<AudioType> action)
        {
            try
            {
                if (audio.Contains(".wav") || audio.Contains(".mp3") || audio.Contains(".ogg"))
                {
                    return AsyncAddCustomAudioType(audio, mod, action);
                }
                else
                {
                    action?.Invoke(EnumUtils.Parse<AudioType>(audio));
                }
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Couldn't load AudioType:\n{e}");
                action?.Invoke(AudioType.None);
            }

            return null;
        }

        // Create a custom audio type from relative file path and the mod
        private static AudioUtilities.AsyncAudioLoader AsyncAddCustomAudioType(string audioPath, IModBehaviour mod, Action<AudioType> action)
        {
            var id = mod.ModHelper.Manifest.UniqueName + "_" + audioPath;
            if (_customAudioTypes.TryGetValue(id, out var audioType))
            {
                action?.Invoke(audioType);
            }

            var audioClipLoader = AudioUtilities.LoadAudio(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, audioPath));

            audioClipLoader.audioLoadedEvent.AddListener((audioClip) =>
            {
                if (audioClip == null)
                {
                    action?.Invoke(AudioType.None);
                }
                else
                {
                    // Could have loaded in the meantime
                    if (_customAudioTypes.TryGetValue(id, out var audioType))
                    {
                        action?.Invoke(audioType);
                    }
                    else
                    {
                        var customAudioType = AddCustomAudioType(id, new AudioClip[] { audioClip });
                        action?.Invoke(customAudioType);
                    }
                }
            });

            return audioClipLoader;
        }

        // Create a custom audio type from a set of audio clips. Needs a unique ID
        private static AudioType AddCustomAudioType(string id, AudioClip[] audioClips)
        {
            NHLogger.LogVerbose($"Registering new audio type [{id}]");

            var audioType = EnumUtilities.Create<AudioType>(id);

            _customAudioTypes.Add(id, audioType);

            var entry = new AudioLibrary.AudioEntry(audioType, audioClips);
            _library.audioEntries = _library.audioEntries.Append(entry).ToArray();
            Locator.GetAudioManager()._audioLibraryDict.Add((int)audioType, entry);

            return audioType;
        }
    }
}
