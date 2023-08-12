using NewHorizons.External.SerializableEnums;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Collections;
using UnityEngine;


namespace NewHorizons.Components.Volumes
{
    internal class LoadCreditsVolume : BaseVolume
    {
        public NHCreditsType creditsType = NHCreditsType.Fast;

        public string gameOverText;
        public DeathType deathType = DeathType.Default;

        private GameOverController _gameOverController;
        private PlayerCameraEffectController _playerCameraEffectController;

        public void Start()
        {
            _gameOverController = FindObjectOfType<GameOverController>();
            _playerCameraEffectController = FindObjectOfType<PlayerCameraEffectController>();
        }

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector") && enabled)
            {
                // Have to run it off the mod behaviour since the game over controller disables everything
                Delay.StartCoroutine(GameOver());
            }
        }

        private IEnumerator GameOver()
        {
            OWInput.ChangeInputMode(InputMode.None);
            ReticleController.Hide();
            Locator.GetPromptManager().SetPromptsVisible(false);
            Locator.GetPauseCommandListener().AddPauseCommandLock();

            // The PlayerCameraEffectController is what actually kills us, so convince it we're already dead
            Locator.GetDeathManager()._isDead = true;

            _playerCameraEffectController.OnPlayerDeath(deathType);
            
            yield return new WaitForSeconds(_playerCameraEffectController._deathFadeLength);

            if (!string.IsNullOrEmpty(gameOverText) && _gameOverController != null)
            {
                _gameOverController._deathText.text = TranslationHandler.GetTranslation(gameOverText, TranslationHandler.TextType.UI);
                _gameOverController.SetupGameOverScreen(5f);

                // We set this to true to stop it from loading the credits scene, so we can do it ourselves
                _gameOverController._loading = true;

                yield return new WaitUntil(ReadytoLoadCreditsScene);
            }

            LoadCreditsScene();
        }

        private bool ReadytoLoadCreditsScene() => _gameOverController._fadedOutText && _gameOverController._textAnimator.IsComplete();

        public override void OnTriggerVolumeExit(GameObject hitObj) { }

        private void LoadCreditsScene()
        {
            NHLogger.LogVerbose($"Load credits {creditsType}");

            switch (creditsType)
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
            }
        }
    }
}
