using NewHorizons.External.Modules;
using NewHorizons.External.SerializableEnums;
using NewHorizons.Handlers;
using NewHorizons.Patches.CreditsScenePatches;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components
{
    public class NHGameOverManager : MonoBehaviour
    {
        /// <summary>
        /// Mod unique id to game over module list
        /// Done as a dictionary so that Reload Configs can overwrite entries per mod
        /// </summary>
        public static Dictionary<IModBehaviour, GameOverModule[]> gameOvers = new();

        public static NHGameOverManager Instance { get; private set; }

        private GameOverController _gameOverController;
        private PlayerCameraEffectController _playerCameraEffectController;

        private (IModBehaviour mod, GameOverModule gameOver)[] _gameOvers;

        private bool _gameOverSequenceStarted;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            _gameOverController = FindObjectOfType<GameOverController>();
            _playerCameraEffectController = FindObjectOfType<PlayerCameraEffectController>();

            var gameOverList = new List<(IModBehaviour, GameOverModule)>();
            foreach (var gameOverPair in gameOvers)
            {
                var mod = gameOverPair.Key;
                foreach (var gameOver in gameOverPair.Value)
                {
                    gameOverList.Add((mod, gameOver));
                }
            }
            _gameOvers = gameOverList.ToArray();
        }

        public void TryHijackDeathSequence()
        {
            var gameOver = _gameOvers.FirstOrDefault(x => !string.IsNullOrEmpty(x.gameOver.condition) 
                && DialogueConditionManager.SharedInstance.GetConditionState(x.gameOver.condition));
            if (!_gameOverSequenceStarted && gameOver != default && !Locator.GetDeathManager()._finishedDLC)
            {
                StartGameOverSequence(gameOver.gameOver, null, gameOver.mod);
            }
        }

        public void StartGameOverSequence(GameOverModule gameOver, DeathType? deathType, IModBehaviour mod)
        {
            _gameOverSequenceStarted = true;
            Delay.StartCoroutine(GameOver(gameOver, deathType, mod));
        }

        private IEnumerator GameOver(GameOverModule gameOver, DeathType? deathType, IModBehaviour mod)
        {
            OWInput.ChangeInputMode(InputMode.None);
            ReticleController.Hide();
            Locator.GetPromptManager().SetPromptsVisible(false);
            Locator.GetPauseCommandListener().AddPauseCommandLock();

            // The PlayerCameraEffectController is what actually kills us, so convince it we're already dead
            Locator.GetDeathManager()._isDead = true;

            var fadeLength = 2f;

            if (Locator.GetDeathManager()._isDying)
            {
                // Player already died at this point, so don't fade
                fadeLength = 0f;
            }
            else if (deathType is DeathType nonNullDeathType)
            {
                _playerCameraEffectController.OnPlayerDeath(nonNullDeathType);
                fadeLength = _playerCameraEffectController._deathFadeLength;
            }
            else
            {
                // Wake up relaxed next loop
                PlayerData.SetLastDeathType(DeathType.Meditation);
                FadeHandler.FadeOut(fadeLength);
            }

            yield return new WaitForSeconds(fadeLength);

            if (!string.IsNullOrEmpty(gameOver.text) && _gameOverController != null)
            {
                _gameOverController._deathText.text = TranslationHandler.GetTranslation(gameOver.text, TranslationHandler.TextType.UI);
                _gameOverController.SetupGameOverScreen(5f);

                if (gameOver.colour != null)
                {
                    _gameOverController._deathText.color = gameOver.colour.ToColor();
                }

                // Make sure the fade handler is off now
                FadeHandler.FadeIn(0f);

                // We set this to true to stop it from loading the credits scene, so we can do it ourselves
                _gameOverController._loading = true;

                yield return new WaitUntil(ReadytoLoadCreditsScene);
            }

            LoadCreditsScene(gameOver, mod);
        }

        private bool ReadytoLoadCreditsScene() => _gameOverController._fadedOutText && _gameOverController._textAnimator.IsComplete();

        private void LoadCreditsScene(GameOverModule gameOver, IModBehaviour mod)
        {
            NHLogger.LogVerbose($"Load credits {gameOver.creditsType}");

            switch (gameOver.creditsType)
            {
                case NHCreditsType.Fast:
                    LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack);
                    break;
                case NHCreditsType.Final:
                    LoadManager.LoadScene(OWScene.Credits_Final, LoadManager.FadeType.ToBlack);
                    break;
                case NHCreditsType.Kazoo:
                    TimelineObliterationController.s_hasRealityEnded = true;
                    LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack);
                    break;
                case NHCreditsType.Custom:
                    LoadCustomCreditsScene(gameOver, mod);
                    break;
                default:
                    // GameOverController disables post processing
                    _gameOverController._flashbackCamera.postProcessing.enabled = true;
                    // For some reason this isn't getting set sometimes
                    AudioListener.volume = 1;
                    GlobalMessenger.FireEvent("TriggerFlashback");
                    break;
            }
        }

        private void LoadCustomCreditsScene(GameOverModule gameOver, IModBehaviour mod)
        {
            LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack);

            // Unfortunately we can't make this a private method, as EventArgs/EventHandler enforces the (sender, e) parameters, which prevents us from passing in gameOver and mod, which we need.
            EventHandler onCreditsBuilt = null; // needs to be done so we can unsubscribe from within the lambda.
            onCreditsBuilt = (sender, e) =>
            {
                // Unsubscribe first, playing it safe in case it NREs 
                CreditsPatches.CreditsBuilt -= onCreditsBuilt;

                // Patch new music clip
                var musicSource = Locator.FindObjectsOfType<OWAudioSource>().Where(x => x.name == "AudioSource").Single(); // AudioSource that plays the credits music is literally called "AudioSource", luckily it's the only one called that. Lazy OW devs do be lazy.
                if (gameOver.audio != string.Empty) // string.Empty is default value for "audio" in GameOverModule, means no audio is specified.
                {
                    AudioUtilities.SetAudioClip(musicSource, gameOver.audio, mod); // Load audio if specified
                }
                else
                {
                    musicSource.AssignAudioLibraryClip(AudioType.PLACEHOLDER); // Otherwise default custom credits are silent
                }

                musicSource.loop = gameOver.audioLooping;
                musicSource._maxSourceVolume = gameOver.audioVolume;

                // Override fade in
                musicSource.Stop();
                musicSource.Play();

                // Patch scroll duration
                var creditsScroll = Locator.FindObjectOfType<CreditsScrollSection>();
                creditsScroll._scrollDuration = gameOver.length;
            };

            CreditsPatches.CreditsBuilt += onCreditsBuilt;
        }
    }
}
