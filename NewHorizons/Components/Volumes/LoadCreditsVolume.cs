using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using System.Collections;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    internal class LoadCreditsVolume : BaseVolume
    {
        public VolumesModule.LoadCreditsVolumeInfo.CreditsType creditsType = VolumesModule.LoadCreditsVolumeInfo.CreditsType.Fast;

        public string gameOverText;
        public DeathType deathType = DeathType.Default;

        private GameOverController _gameOverController;
        private PlayerCameraEffectController _playerCameraEffectController;

        public void Start()
        {
            _gameOverController = GameObject.FindObjectOfType<GameOverController>();
            _playerCameraEffectController = GameObject.FindObjectOfType<PlayerCameraEffectController>();
        }

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector") && enabled)
            {
                StartCoroutine(GameOver());
            }
        }

        private IEnumerator GameOver()
        {
            OWInput.ChangeInputMode(InputMode.None);
            ReticleController.Hide();
            Locator.GetPromptManager().SetPromptsVisible(false);
            Locator.GetPauseCommandListener().AddPauseCommandLock();

            _playerCameraEffectController.OnPlayerDeath(deathType);
            
            yield return new WaitForSeconds(_playerCameraEffectController._deathFadeLength);

            if (!string.IsNullOrEmpty(gameOverText) && _gameOverController != null)
            {
                _gameOverController._deathText.text = TranslationHandler.GetTranslation(gameOverText, TranslationHandler.TextType.UI);
                _gameOverController.SetupGameOverScreen(5f);

                // We set this to true to stop it from loading the credits scene, so we can do it ourselves
                _gameOverController._loading = true;

                yield return new WaitUntil(ReadytoLoadCreditsScene);

                LoadCreditsScene();
            }
            else
            {
                LoadCreditsScene();
            }
        }

        private bool ReadytoLoadCreditsScene() => _gameOverController._fadedOutText && _gameOverController._textAnimator.IsComplete();

        public override void OnTriggerVolumeExit(GameObject hitObj) { }

        private void LoadCreditsScene()
        {
            switch (creditsType)
            {
                case VolumesModule.LoadCreditsVolumeInfo.CreditsType.Fast:
                    LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack);
                    break;
                case VolumesModule.LoadCreditsVolumeInfo.CreditsType.Final:
                    LoadManager.LoadScene(OWScene.Credits_Final, LoadManager.FadeType.ToBlack);
                    break;
                case VolumesModule.LoadCreditsVolumeInfo.CreditsType.Kazoo:
                    TimelineObliterationController.s_hasRealityEnded = true;
                    LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack);
                    break;
            }
        }
    }
}
