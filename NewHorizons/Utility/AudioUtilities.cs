using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NewHorizons.Utility
{
    public static class AudioUtilities
    {
        private static readonly Dictionary<string, AudioClip> _loadedAudioClips = new Dictionary<string, AudioClip>();

        public static AudioClip LoadAudio(string path)
        {
            if (_loadedAudioClips.ContainsKey(path))
            {
                Logger.Log($"Already loaded audio at path: {path}");
                return _loadedAudioClips[path];
            }

            Logger.Log($"Loading audio at path: {path}");
            var task = Task.Run(async () => await GetAudioClip(path));
            task.Wait();
            _loadedAudioClips.Add(path, task.Result);
            return task.Result;
        }

        public static void ClearCache()
        {
            Logger.Log("Clearing audio cache");

            foreach (var audioClip in _loadedAudioClips.Values)
            {
                if (audioClip == null) continue;
                Object.Destroy(audioClip);
            }

            _loadedAudioClips.Clear();
        }

        private static async Task<AudioClip> GetAudioClip(string filePath)
        {
            var extension = filePath.Split('.').Last();

            UnityEngine.AudioType audioType;

            switch (extension)
            {
                case "wav":
                    audioType = UnityEngine.AudioType.WAV;
                    break;
                case "ogg":
                    audioType = UnityEngine.AudioType.OGGVORBIS;
                    break;
                default:
                    Logger.LogError($"Invalid audio file extension ({extension}) must be .wav or .ogg");
                    return null;
            }

            using (var www = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType))
            {
                var result = www.SendWebRequest();

                while (!result.isDone) await Task.Delay(100);

                if (www.isNetworkError)
                {
                    Debug.Log(www.error);
                    return null;
                }

                return DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }
}