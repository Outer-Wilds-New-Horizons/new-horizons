using NewHorizons.External.Modules;
using NewHorizons.External.SerializableEnums;
using NewHorizons.Handlers;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class NHGameOverManager : MonoBehaviour
    {
        /// <summary>
        /// Mod unique id to game over module list
        /// Done as a dictionary so that Reload Configs can overwrite entries per mod
        /// </summary>
        public static Dictionary<string, GameOverModule[]> gameOvers = new();

        public static NHGameOverManager Instance { get; private set; }

        private GameOverController _gameOverController;
        private PlayerCameraEffectController _playerCameraEffectController;

        private GameOverModule[] _gameOvers;

        private bool _gameOverSequenceStarted;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            _gameOverController = FindObjectOfType<GameOverController>();
            _playerCameraEffectController = FindObjectOfType<PlayerCameraEffectController>();

            _gameOvers = gameOvers.SelectMany(x => x.Value).ToArray();
        }

        public void TryHijackDeathSequence()
        {
            var gameOver = _gameOvers.FirstOrDefault(x => !string.IsNullOrEmpty(x.condition) && DialogueConditionManager.SharedInstance.GetConditionState(x.condition));
            if (!_gameOverSequenceStarted && gameOver != null && !Locator.GetDeathManager()._finishedDLC)
            {
                StartGameOverSequence(gameOver, null);
            }
        }

        public void StartGameOverSequence(GameOverModule gameOver, DeathType? deathType, IModBehaviour mod = null)
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
                    // We can't load in custom music if an IModBehaviour cannot be provided. This should only happen if called via TryHijackDeathSequence().
                    if (mod is null)
                    {
                        NHLogger.LogWarning("Credits called using TryHijackDeathSequence(), custom credits audio cannot not be loaded.");
                    }
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
            var fromScene = LoadManager.GetCurrentScene();
            var toScene = OWScene.Credits_Fast;

            LoadManager.LoadScene(toScene, LoadManager.FadeType.ToBlack);

            // We need to do this so we can unsubscribe from within the lambda.
            LoadManager.SceneLoadEvent completeCreditsLoad = null;

            completeCreditsLoad = (fromScene, toScene) =>
            {
                // Patch new music
                var musicSource = Locator.FindObjectsOfType<OWAudioSource>().Where(x => x.name == "AudioSource").Single();
                musicSource.Stop();
                if (mod is not null)
                {
                    AudioUtilities.SetAudioClip(musicSource, gameOver.audio, mod);
                }
                musicSource.SetMaxVolume(gameOver.audioVolume);
                musicSource.loop = gameOver.audioLooping;
                musicSource.FadeIn(gameOver.audioFadeInLength);

                // Janky wait until credits are built
                Task.Run( () =>
                {
                    var startTime = Time.time;
                    while (Locator.FindObjectsOfType<CreditsScrollSection>().Length == 0) {
                        if (Time.time > startTime + 0.1f)
                        {
                            NHLogger.LogError("Timeout while waiting for credits to be built. Scroll duration couldn't be changed.");
                            return;
                        }
                    }
                    
                    // Patch scroll duration
                    var creditsScroll = Locator.FindObjectOfType<CreditsScrollSection>();
                    creditsScroll._scrollDuration = gameOver.scrollDuration;
                });

                LoadManager.OnCompleteSceneLoad -= completeCreditsLoad;
            };

            LoadManager.OnCompleteSceneLoad += completeCreditsLoad;
        }
    }
}
