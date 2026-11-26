using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static NewHorizons.Utility.Files.AssetBundleUtilities;

namespace NewHorizons.Components.ShipLog
{
    public class ShipLogLegacyStarChartMode : ShipLogMode, IShipLogStarChartMode
    {
        private List<GameObject> _starSystemCards = new List<GameObject>();
        private GameObject _cardTemplate = null;
        private int _cardIndex = 0;
        private OWAudioSource _oneShotSource;
        private FontAndLanguageController _fontAndLanguageController;

        private float _startPanTime;
        private float _panDuration;
        private Transform root;
        private Vector2 _panRootPos = Vector2.zero;
        private Vector2 _startPanPos;

        private ScreenPromptList _centerPromptList;

        private ScreenPrompt _targetSystemPrompt;
        private ScreenPrompt _warpPrompt = new ScreenPrompt(InputLibrary.autopilot, "<CMD> Warp to system");

        private ShipLogEntryCard _target = null;
        private NotificationData _warpNotificationData = null;

        private float _volumeScale = 0.45f;
        private AudioClip _onOpenClip;
        private AudioClip _onSelectClip;
        private AudioClip _onDeselectClip;

        private int _nextCardIndex;

        private HashSet<string> _systemCards = new();

        private void Awake()
        {
            // Prompts
            Locator.GetPromptManager().AddScreenPrompt(_warpPrompt, PromptPosition.UpperLeft, false);
            _systemCards.Clear();
        }

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            _fontAndLanguageController = GetComponentInParent<ShipLogController>().GetComponentInChildren<FontAndLanguageController>(true);

            root = transform.Find("ScaleRoot/PanRoot");
            _oneShotSource = oneShotSource;
            LoadAssets();

            _centerPromptList = centerPromptList;

            _targetSystemPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, TranslationHandler.GetTranslation("LOCK_AUTOPILOT_WARP", TranslationHandler.TextType.UI), 0, ScreenPrompt.DisplayState.Normal, false);

            GlobalMessenger<ReferenceFrame>.AddListener("TargetReferenceFrame", new Callback<ReferenceFrame>(OnTargetReferenceFrame));

            _nextCardIndex = 0;
            foreach (var starSystem in Main.SystemDict.Keys)
            {
                if (StarChartHandler.CanWarpToSystem(starSystem))
                {
                    AddStarSystem(starSystem);
                }
            }

            /*
            if(VesselCoordinatePromptHandler.KnowsEyeCoordinates())
            {
                AddSystemCard("EyeOfTheUniverse");
            }
            */
        }

        private void LoadAssets()
        {
            _onOpenClip = NHPrivateAssetBundle.LoadAsset<AudioClip>("Assets/StarChart/Audio/open star map.ogg");
            _onSelectClip = NHPrivateAssetBundle.LoadAsset<AudioClip>("Assets/StarChart/Audio/select star.ogg");
            _onDeselectClip = NHPrivateAssetBundle.LoadAsset<AudioClip>("Assets/StarChart/Audio/deselect star.ogg");
        }

        public void AddStarSystem(string uniqueID)
        {
            if (!_systemCards.Contains(uniqueID))
            {
                var card = CreateCard(uniqueID, root.transform, new Vector2(_nextCardIndex++ * 200, 0));
                _starSystemCards.Add(card);
            }
            else
            {
                NHLogger.LogWarning($"Tried making duplicate system card {uniqueID}");
            }
        }

        public void OnDestroy()
        {
            GlobalMessenger<ReferenceFrame>.RemoveListener("TargetReferenceFrame", new Callback<ReferenceFrame>(OnTargetReferenceFrame));

            Locator.GetPromptManager()?.RemoveScreenPrompt(_warpPrompt, PromptPosition.UpperLeft);
        }

        public GameObject CreateCard(string uniqueID, Transform parent, Vector2 position)
        {
            if (_cardTemplate == null)
            {
                var panRoot = SearchUtilities.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ScaleRoot/PanRoot");
                _cardTemplate = Instantiate(panRoot.GetComponentInChildren<ShipLogEntryCard>(true).gameObject);
                _cardTemplate.SetActive(false);
            }

            var newCard = Instantiate(_cardTemplate, parent);

            var name = ShipLogStarChartMode.UniqueIDToName(uniqueID);

            newCard.SetActive(true);
            newCard.transform.name = uniqueID;
            newCard.transform.localPosition = new Vector3(position.x, position.y, 40);
            newCard.transform.localRotation = Quaternion.Euler(0, 0, 0);

            var shipLogEntryCard = newCard.GetComponent<ShipLogEntryCard>();

            shipLogEntryCard._name.text = name;
            if (name.Length > 17) shipLogEntryCard._name.fontSize = 10;
            else shipLogEntryCard._name.fontSize = 14;

            shipLogEntryCard._name.font = Locator.GetUIStyleManager().GetShipLogCardFont();
            shipLogEntryCard._name.lineSpacing = Locator.GetUIStyleManager().GetShipLogCardSpacing();
            shipLogEntryCard._questionMark.color = Locator.GetUIStyleManager().GetShipLogRumorColor();

            Texture texture = StarChartHandler.GetSystemCardTexture(uniqueID);

            if (texture != null)
            {
                shipLogEntryCard._photo.sprite = MakeSprite((Texture2D)texture);
                shipLogEntryCard._photo.gameObject.SetActive(true);
            }

            shipLogEntryCard._hudMarkerIcon.gameObject.SetActive(false);
            shipLogEntryCard._moreToExploreIcon.gameObject.SetActive(false);
            shipLogEntryCard._unreadIcon.gameObject.SetActive(false);
            shipLogEntryCard._origIconSize = shipLogEntryCard._moreToExploreIcon.rectTransform.sizeDelta;

            _fontAndLanguageController.AddTextElement(shipLogEntryCard._name, false, true, false);
            shipLogEntryCard._name.SetAllDirty();
            shipLogEntryCard._questionMark.SetAllDirty();
            shipLogEntryCard._photo.SetAllDirty();

            return newCard;
        }

        public override bool AllowCancelInput()
        {
            return true;
        }

        public override bool AllowModeSwap()
        {
            return true;
        }

        public override void EnterMode(string entryID = "", List<ShipLogFact> revealQueue = null)
        {
            gameObject.SetActive(true);

            _oneShotSource.PlayOneShot(_onOpenClip, _volumeScale);
            Locator.GetPromptManager().AddScreenPrompt(_targetSystemPrompt, _centerPromptList, TextAnchor.MiddleCenter, -1, true);
        }

        public override void ExitMode()
        {
            gameObject.SetActive(false);

            Locator.GetPromptManager().RemoveScreenPrompt(_targetSystemPrompt);
        }

        public override string GetFocusedEntryID()
        {
            return "";
        }

        public override void OnEnterComputer()
        {

        }

        public override void OnExitComputer()
        {

        }

        public override void UpdateMode()
        {
            UpdateMapCamera();
            UpdateMapNavigation();
            UpdatePrompts();
        }

        private void UpdateMapCamera()
        {
            if (_starSystemCards.Count == 0)
            {
                NHLogger.LogWarning("Showing star chart mode when there are no available systems");
                return;
            }

            Vector2 b = -_starSystemCards[_cardIndex].transform.localPosition;
            float num = Mathf.InverseLerp(_startPanTime, _startPanTime + _panDuration, Time.unscaledTime);
            num = 1f - (num - 1f) * (num - 1f);
            _panRootPos = Vector2.Lerp(_startPanPos, b, num);
            root.transform.localPosition = new Vector3(_panRootPos.x, _panRootPos.y, 0);
        }

        private void UpdateMapNavigation()
        {
            var oldIndex = _cardIndex;
            if (OWInput.IsNewlyPressed(InputLibrary.right, InputMode.All) || OWInput.IsNewlyPressed(InputLibrary.right2, InputMode.All))
            {
                _cardIndex = Posmod(_cardIndex + 1, _starSystemCards.Count());
            }
            else if (OWInput.IsNewlyPressed(InputLibrary.left, InputMode.All) || OWInput.IsNewlyPressed(InputLibrary.left2, InputMode.All))
            {
                _cardIndex = Posmod(_cardIndex - 1, _starSystemCards.Count());
            }

            if (oldIndex != _cardIndex)
            {
                _oneShotSource.PlayOneShot(AudioType.ShipLogMoveBetweenPlanets, 1f);
                _startPanTime = Time.unscaledTime;
                _startPanPos = _panRootPos;
                _panDuration = 0.25f;
            }
        }

        private void UpdatePrompts()
        {
            if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD, InputMode.All))
            {
                var shipLogEntryCard = _starSystemCards[_cardIndex].GetComponent<ShipLogEntryCard>();

                if (_target == shipLogEntryCard) RemoveWarpTarget(true);
                else SetWarpTarget(shipLogEntryCard);
            }
        }

        private int Posmod(int a, int b)
        {
            return (a % b + b) % b;
        }

        private Sprite MakeSprite(Texture2D texture)
        {
            var rect = new Rect(0, 0, texture.width, texture.height);
            var pivot = new Vector2(texture.width / 2, texture.height / 2);
            return Sprite.Create(texture, rect, pivot, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);
        }

        private void OnTargetReferenceFrame(ReferenceFrame referenceFrame)
        {
            RemoveWarpTarget();
        }

        private void SetWarpTarget(ShipLogEntryCard shipLogEntryCard)
        {
            RemoveWarpTarget(false);
            _oneShotSource.PlayOneShot(_onSelectClip, _volumeScale);
            _target = shipLogEntryCard;
            _target.SetMarkedOnHUD(true);
            Locator._rfTracker.UntargetReferenceFrame();
            GlobalMessenger.FireEvent("UntargetReferenceFrame");

            var name = ShipLogStarChartMode.UniqueIDToName(shipLogEntryCard.name);

            var warpNotificationDataText = TranslationHandler.GetTranslation("WARP_LOCKED", TranslationHandler.TextType.UI).Replace("{0}", name.ToUpperFixed());
            _warpNotificationData = new NotificationData(warpNotificationDataText);
            NotificationManager.SharedInstance.PostNotification(_warpNotificationData, true);

            var warpPromptText = "<CMD> " + TranslationHandler.GetTranslation("ENGAGE_WARP_PROMPT", TranslationHandler.TextType.UI).Replace("{0}", name);
            _warpPrompt.SetText(warpPromptText);
        }

        private void RemoveWarpTarget(bool playSound = false)
        {
            if (_warpNotificationData != null) NotificationManager.SharedInstance.UnpinNotification(_warpNotificationData);
            if (_target == null) return;
            if (playSound) _oneShotSource.PlayOneShot(_onDeselectClip, _volumeScale);
            _target.SetMarkedOnHUD(false);
            _target = null;
        }

        public string GetTargetStarSystem()
        {
            return _target?.name;
        }

        private bool IsWarpDriveAvailable()
        {
            return OWInput.IsInputMode(InputMode.ShipCockpit) && _target != null;
        }

        public void UpdateWarpPromptVisibility()
        {
            _warpPrompt.SetVisibility(IsWarpDriveAvailable());
        }
    }
}