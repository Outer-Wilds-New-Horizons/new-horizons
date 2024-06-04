using NewHorizons.Utility.OWML;
using System.Collections;
using UnityEngine;

namespace NewHorizons.Components.Props
{
    public class ConditionalObjectActivation : MonoBehaviour
    {
        private bool _playerAwake, _playerDoneAwake;

        public GameObject GameObject;
        public string DialogueCondition;
        public bool CloseEyes;
        public bool SetActiveWithCondition;

        private PlayerCameraEffectController _playerCameraEffectController;
        private bool _changeConditionOnExitConversation;
        private bool _inConversation;

        public static void SetUp(GameObject go, string condition, bool closeEyes, bool setActiveWithCondition)
        {
            var conditionalObjectActivationGO = new GameObject($"{go.name}_{condition}");
            var component = conditionalObjectActivationGO.AddComponent<ConditionalObjectActivation>();
            component.transform.parent = go.transform.parent;
            component.GameObject = go;
            component.DialogueCondition = condition;
            component.CloseEyes = closeEyes;
            component.SetActiveWithCondition = setActiveWithCondition;
        }

        public void Start()
        {
            // We delay else some props don't get properly initialized before being disappeared
            Delay.FireOnNextUpdate(LateStart);
        }

        private void LateStart()
        {
            var currentConditionState = DialogueConditionManager.SharedInstance.GetConditionState(DialogueCondition);

            // Would just call OnDialogueConditionChanged but maybe theres an activator and deactivator for this object so we have to be more careful
            if (SetActiveWithCondition && !currentConditionState) GameObject.SetActive(false);
            if (!SetActiveWithCondition && currentConditionState) GameObject.SetActive(false);
        }

        public void Awake()
        {
            _playerCameraEffectController = GameObject.FindObjectOfType<PlayerCameraEffectController>();
            GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);
            GlobalMessenger.AddListener("ExitConversation", OnExitConversation);
            GlobalMessenger.AddListener("EnterConversation", OnEnterConversation);
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
        }

        public void OnDestroy()
        {
            GlobalMessenger<string, bool>.RemoveListener("DialogueConditionChanged", OnDialogueConditionChanged);
            GlobalMessenger.RemoveListener("ExitConversation", OnExitConversation);
            GlobalMessenger.RemoveListener("EnterConversation", OnEnterConversation);
            GlobalMessenger.RemoveListener("WakeUp", OnWakeUp);
        }

        private void OnWakeUp()
        {
            _playerAwake = true;
        }

        public void Update()
        {
            if (!_playerDoneAwake && _playerAwake)
            {
                if (!_playerCameraEffectController._isOpeningEyes)
                {
                    _playerDoneAwake = true;
                }
            }
        }

        public void OnExitConversation()
        {
            _inConversation = false;
            if (_changeConditionOnExitConversation)
            {
                OnDialogueConditionChanged(DialogueCondition, DialogueConditionManager.SharedInstance.GetConditionState(DialogueCondition));
                _changeConditionOnExitConversation = false;
            }
        }

        public void OnEnterConversation()
        {
            _inConversation = true;
        }

        public void OnDialogueConditionChanged(string condition, bool state)
        {
            if (condition == DialogueCondition)
            {
                if (_inConversation)
                {
                    _changeConditionOnExitConversation = true;
                }
                else
                {
                    SetActive(SetActiveWithCondition == state);
                }
            }
        }

        public void SetActive(bool active)
        {
            if (CloseEyes && _playerDoneAwake && LateInitializerManager.isDoneInitializing)
            {
                Delay.StartCoroutine(Coroutine(active));
            }
            else
            {
                GameObject.SetActive(active);
            }
        }

        private IEnumerator Coroutine(bool active)
        {
            OWInput.ChangeInputMode(InputMode.None);
            Locator.GetPauseCommandListener().AddPauseCommandLock();

            _playerCameraEffectController.CloseEyes(0.7f);
            yield return new WaitForSeconds(0.7f);

            // Eyes closed: swap character state
            GameObject.SetActive(active);

            yield return new WaitForSeconds(0.3f);

            // Open eyes
            _playerCameraEffectController.OpenEyes(0.7f);

            OWInput.ChangeInputMode(InputMode.Character);
            Locator.GetPauseCommandListener().RemovePauseCommandLock();
        }
    }
}
