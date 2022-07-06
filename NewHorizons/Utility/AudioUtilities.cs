using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace NewHorizons.Utility
{
    public static class AudioUtilities
    {
        private static Dictionary<string, AudioClip> _loadedAudioClips = new Dictionary<string, AudioClip>();

        public static AudioClip LoadAudio(string path)
        {
            if (_loadedAudioClips.ContainsKey(path))
            {
                Logger.LogVerbose($"Already loaded audio at path: {path}");
                return _loadedAudioClips[path];
            }
            Logger.LogVerbose($"Loading audio at path: {path}");
            var task = Task.Run(async () => await GetAudioClip(path));
            task.Wait();
            _loadedAudioClips.Add(path, task.Result);
            return task.Result;
        }

        public static void ClearCache()
        {
            Logger.LogVerbose("Clearing audio cache");

            foreach (var audioClip in _loadedAudioClips.Values)
            {
                if (audioClip == null) continue;
                UnityEngine.Object.Destroy(audioClip);
            }
            _loadedAudioClips.Clear();
        }

        private static async Task<AudioClip> GetAudioClip(string filePath)
        {
            var extension = filePath.Split(new char[] { '.' }).Last();

            UnityEngine.AudioType audioType;

            switch (extension)
            {
                case ("wav"):
                    audioType = UnityEngine.AudioType.WAV;
                    break;
                case ("ogg"):
                    audioType = UnityEngine.AudioType.OGGVORBIS;
                    break;
                case ("mp3"):
                    audioType = UnityEngine.AudioType.MPEG;
                    break;
                default:
                    Logger.LogError($"Invalid audio file extension ({extension}) must be .wav or .ogg or .mp3");
                    return null;
            }

            if (audioType == UnityEngine.AudioType.MPEG)
            {
                string fileProtocolPath = $"file://{filePath}";
                DownloadHandlerAudioClip dh = new DownloadHandlerAudioClip(fileProtocolPath, UnityEngine.AudioType.MPEG);
                dh.compressed = true;
                using (UnityWebRequest www = new UnityWebRequest(fileProtocolPath, "GET", dh, null))
                {
                    var result = www.SendWebRequest();

                    while (!result.isDone) { await Task.Delay(100); }

                    if (www.isNetworkError)
                    {
                        Debug.Log(www.error);
                        return null;
                    }
                    else
                    {
                        return dh.audioClip;
                    }
                }
            }
            else
            {
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType))
                {
                    var result = www.SendWebRequest();

                    while (!result.isDone) { await Task.Delay(100); }

                    if (www.isNetworkError)
                    {
                        Debug.Log(www.error);
                        return null;
                    }
                    else
                    {
                        return DownloadHandlerAudioClip.GetContent(www);
                    }
                }
            }
        }
    }
}
