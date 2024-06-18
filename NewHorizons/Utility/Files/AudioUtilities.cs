using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace NewHorizons.Utility.Files
{
    public static class AudioUtilities
    {
        private static Dictionary<string, AudioClip> _loadedAudioClips = new Dictionary<string, AudioClip>();

        public static void SetAudioClip(OWAudioSource source, string audio, IModBehaviour mod)
        {
            if (string.IsNullOrWhiteSpace(audio)) return;

            if (audio.Contains(".wav") || audio.Contains(".ogg") || audio.Contains(".mp3"))
            {
                try
                {
                    var clip = LoadAudio(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, audio));
                    source._audioLibraryClip = AudioType.None;
                    source._clipArrayIndex = 0;
                    source._clipArrayLength = 0;
                    source._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.MANUAL;
                    source.clip = clip;
                    return;
                }
                catch
                {
                    NHLogger.LogError($"Could not load file {audio}");
                }
            }

            if (EnumUtils.TryParse(audio, out AudioType type))
            {
                source._audioLibraryClip = type;
            }
            else
            {
                var audioClip = SearchUtilities.FindResourceOfTypeAndName<AudioClip>(audio);
                if (audioClip == null) NHLogger.Log($"Couldn't find audio clip {audio}");
                else source.clip = audioClip;
            }
        }

        public static AudioClip LoadAudio(string path)
        {
            try
            {
                if (_loadedAudioClips.ContainsKey(path))
                {
                    NHLogger.LogVerbose($"Already loaded audio at path: {path}");
                    return _loadedAudioClips[path];
                }
                NHLogger.LogVerbose($"Loading audio at path: {path}");
                var task = Task.Run(async () => await GetAudioClip(path));
                task.Wait();
                _loadedAudioClips.Add(path, task.Result);
                return task.Result;
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Couldn't load Audio at {path} : {ex}");
                return null;
            }
        }

        public static void ClearCache()
        {
            NHLogger.LogVerbose("Clearing audio cache");

            foreach (var audioClip in _loadedAudioClips.Values)
            {
                if (audioClip == null) continue;
                UnityEngine.Object.Destroy(audioClip);
            }
            _loadedAudioClips.Clear();
        }

        private static async Task<AudioClip> GetAudioClip(string path)
        {
            var extension = Path.GetExtension(path);

            UnityEngine.AudioType audioType;

            switch (extension)
            {
                case ".wav":
                    audioType = UnityEngine.AudioType.WAV;
                    break;
                case ".ogg":
                    audioType = UnityEngine.AudioType.OGGVORBIS;
                    break;
                case ".mp3":
                    audioType = UnityEngine.AudioType.MPEG;
                    break;
                default:
                    NHLogger.LogError($"Couldn't load Audio at {path} : Invalid audio file extension ({extension}) must be .wav or .ogg or .mp3");
                    return null;
            }

            path = $"file:///{path.Replace("+", "%2B")}";
            if (audioType == UnityEngine.AudioType.MPEG)
            {
                DownloadHandlerAudioClip dh = new DownloadHandlerAudioClip(path, UnityEngine.AudioType.MPEG);
                dh.compressed = true;
                using (UnityWebRequest www = new UnityWebRequest(path, "GET", dh, null))
                {
                    var result = www.SendWebRequest();

                    while (!result.isDone) await Task.Yield();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        NHLogger.LogError($"Couldn't load Audio at {path} : {www.error}");
                        return null;
                    }
                    else
                    {
                        var audioClip = dh.audioClip;
                        audioClip.name = Path.GetFileNameWithoutExtension(path);
                        return audioClip;
                    }
                }
            }
            else
            {
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
                {
                    var result = www.SendWebRequest();

                    while (!result.isDone) await Task.Yield();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        NHLogger.LogError($"Couldn't load Audio at {path} : {www.error}");
                        return null;
                    }
                    else
                    {
                        var audioClip = DownloadHandlerAudioClip.GetContent(www);
                        audioClip.name = Path.GetFileNameWithoutExtension(path);
                        return audioClip;
                    }
                }
            }
        }
    }
}
