using NewHorizons.External.Modules;
using NewHorizons.External.SerializableEnums;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
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

        public void StartGameOverSequence(GameOverModule gameOver, DeathType? deathType)
        {
            _gameOverSequenceStarted = true;
            Delay.StartCoroutine(GameOver(gameOver, deathType));
        }

        private IEnumerator GameOver(GameOverModule gameOver, DeathType? deathType)
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

            LoadCreditsScene(gameOver);
        }

        private bool ReadytoLoadCreditsScene() => _gameOverController._fadedOutText && _gameOverController._textAnimator.IsComplete();

        private void LoadCreditsScene(GameOverModule gameOver)
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
                default:
                    // GameOverController disables post processing
                    _gameOverController._flashbackCamera.postProcessing.enabled = true;
                    // For some reason this isn't getting set sometimes
                    AudioListener.volume = 1;
                    GlobalMessenger.FireEvent("TriggerFlashback");
                    break;
            }
        }
    }
}
