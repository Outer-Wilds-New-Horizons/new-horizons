using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace NewHorizons.Utility.Files;

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
                var audioClipLoader = LoadAudio(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, audio));
                source._audioLibraryClip = AudioType.None;
                source._clipArrayIndex = 0;
                source._clipArrayLength = 0;
                source._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.MANUAL;
                audioClipLoader.audioLoadedEvent.AddListener((audioClip) =>
                {
                    source.clip = audioClip;
                });
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

    public static AsyncAudioLoader LoadAudio(string path)
    {
        var audioLoader = new AsyncAudioLoader(path);
        audioLoader.Load();
        return audioLoader;
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

    public static Action allLoadersComplete;

    public class AsyncAudioLoader
    {
        public class AudioLoadedEvent : UnityEvent<AudioClip> { }

        public AudioLoadedEvent audioLoadedEvent = new();

        public AsyncAudioLoader(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }

        public void Load()
        {
            if (SingletonAudioLoader.Instance == null)
            {
                Main.Instance.gameObject.AddComponent<SingletonAudioLoader>();
            }

            SingletonAudioLoader.Instance.Load(this);
        }
    }

    private class SingletonAudioLoader : MonoBehaviour
    {
        public static SingletonAudioLoader Instance { get; private set; }

        public int loadingCount = 0;

        public void Awake()
        {
            Instance = this;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene _)
        {
            StopAllCoroutines();
            loadingCount = 0;
        }

        public void Load(AsyncAudioLoader loader)
        {
            loadingCount++;
            StartCoroutine(GetAudioClip(loader)); 
        }

        private IEnumerator GetAudioClip(AsyncAudioLoader loader)
        {
            // Wait at least one frame to allow it to subscribe to events
            yield return null;

            var path = loader.Path;

            if (_loadedAudioClips.ContainsKey(path))
            {
                NHLogger.LogVerbose($"Already loaded audio at path: {path}");
                loader.audioLoadedEvent?.Invoke(_loadedAudioClips[path]);
            }
            else
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
                        throw new Exception($"Couldn't load Audio at {path} : Invalid audio file extension ({extension}) must be .wav or .ogg or .mp3");
                }

                path = $"file:///{path.Replace("+", "%2B")}";

                if (audioType == UnityEngine.AudioType.MPEG)
                {
                    DownloadHandlerAudioClip dh = new DownloadHandlerAudioClip(path, UnityEngine.AudioType.MPEG);
                    dh.compressed = true;
                    using UnityWebRequest www = new UnityWebRequest(path, "GET", dh, null);
                    yield return www.SendWebRequest();

                    // Could have loaded in the meantime
                    if (_loadedAudioClips.ContainsKey(path))
                    {
                        NHLogger.LogVerbose($"Already loaded audio at {path}");
                        loader.audioLoadedEvent?.Invoke(_loadedAudioClips[path]);
                    }
                    else
                    {
                        if (www.isNetworkError || www.isHttpError)
                        {
                            NHLogger.LogError($"Couldn't load Audio at {path} : {www.error}");
                            loader.audioLoadedEvent?.Invoke(null);
                        }
                        else
                        {
                            var audioClip = dh.audioClip;
                            audioClip.name = Path.GetFileNameWithoutExtension(path);
                            _loadedAudioClips.Add(path, audioClip);
                            loader.audioLoadedEvent?.Invoke(audioClip);
                        }
                    }
                }
                else
                {
                    using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
                    yield return www.SendWebRequest();

                    // Could have loaded in the meantime
                    if (_loadedAudioClips.ContainsKey(path))
                    {
                        NHLogger.LogVerbose($"Already loaded audio at {path}");
                        loader.audioLoadedEvent?.Invoke(_loadedAudioClips[path]);
                    }
                    else
                    {
                        if (www.isNetworkError || www.isHttpError)
                        {
                            NHLogger.LogError($"Couldn't load Audio at {path} : {www.error}");
                            loader.audioLoadedEvent?.Invoke(null);
                        }
                        else
                        {
                            var audioClip = DownloadHandlerAudioClip.GetContent(www);
                            audioClip.name = Path.GetFileNameWithoutExtension(path);
                            _loadedAudioClips.Add(path, audioClip);
                            loader.audioLoadedEvent?.Invoke(audioClip);
                        }
                    }
                }
            }

            loadingCount--;

            if (loadingCount == 0)
            {
                allLoadersComplete?.Invoke();
            }
        }
    }
}
