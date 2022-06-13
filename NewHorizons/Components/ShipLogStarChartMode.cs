using NewHorizons.Handlers;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Components
{
    public class ShipLogStarChartMode : ShipLogMode
    {
        private List<GameObject> _starSystemCards = new List<GameObject>();
        private GameObject _cardTemplate = null;
        private int _cardIndex = 0;
        private OWAudioSource _oneShotSource;

        private float _startPanTime;
        private float _panDuration;
        private Transform root;
        private Vector2 _panRootPos = Vector2.zero;
        private Vector2 _startPanPos;

        private ScreenPromptList _upperRightPromptList;
        private ScreenPromptList _centerPromptList;

        private ScreenPrompt _detectiveModePrompt;
        private ScreenPrompt _targetSystemPrompt;
        private ScreenPrompt _warpPrompt = new ScreenPrompt(InputLibrary.autopilot, "<CMD> Warp to system");

        private ShipLogEntryCard _target = null;
        private NotificationData _warpNotificationData = null;

        private int _nextCardIndex;

        private void Awake()
        {
            // Prompts
            Locator.GetPromptManager().AddScreenPrompt(_warpPrompt, PromptPosition.UpperLeft, false);
        }

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            root = base.transform.Find("ScaleRoot/PanRoot");
            _oneShotSource = oneShotSource;

            _centerPromptList = centerPromptList;
            _upperRightPromptList = upperRightPromptList;

            _detectiveModePrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, "Rumor Mode", 0, ScreenPrompt.DisplayState.Normal, false);
            _targetSystemPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "Lock Autopilot to Star System", 0, ScreenPrompt.DisplayState.Normal, false);

            GlobalMessenger<ReferenceFrame>.AddListener("TargetReferenceFrame", new Callback<ReferenceFrame>(OnTargetReferenceFrame));

            _nextCardIndex = 0;
            foreach (var starSystem in Main.SystemDict.Keys)
            {
                // Get rid of the warp option for the current system
                if (starSystem == Main.Instance.CurrentStarSystem) continue;

                var config = Main.SystemDict[starSystem];

                // Conditions to allow warping into that system (either no planets (stock system) or has a ship spawn point)
                var flag = false;
                if (starSystem.Equals("SolarSystem")) flag = true;
                else if (config.Spawn?.shipSpawnPoint != null) flag = true;

                if (!StarChartHandler.HasUnlockedSystem(starSystem)) continue;

                if (flag && Main.SystemDict[starSystem].Config.canEnterViaWarpDrive)
                {
                    AddSystemCard(starSystem);
                }
            }

            //AddSystemCard("EyeOfTheUniverse");

            /* Ship log manager isnt initiatiized yet
            if(Locator.GetShipLogManager().IsFactRevealed("OPC_EYE_COORDINATES_X1"))
            {
                AddSystemCard("EyeOfTheUniverse");
            }
            */
        }

        public void AddSystemCard(string uniqueID)
        {
            var card = CreateCard(uniqueID, root.transform, new Vector2(_nextCardIndex++ * 200, 0));
            _starSystemCards.Add(card);
        }

        public void OnDestroy()
        {
            GlobalMessenger<ReferenceFrame>.RemoveListener("TargetReferenceFrame", new Callback<ReferenceFrame>(OnTargetReferenceFrame));

            Locator.GetPromptManager().RemoveScreenPrompt(_warpPrompt, PromptPosition.UpperLeft);
        }

        public GameObject CreateCard(string uniqueID, Transform parent, Vector2 position)
        {
            if (_cardTemplate == null)
            {
                var panRoot = SearchUtilities.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ScaleRoot/PanRoot");
                _cardTemplate = GameObject.Instantiate(panRoot.GetComponentInChildren<ShipLogEntryCard>().gameObject);
                _cardTemplate.SetActive(false);
            }

            var newCard = GameObject.Instantiate(_cardTemplate, parent);
            var textComponent = newCard.transform.Find("EntryCardRoot/NameBackground/Name").GetComponent<Text>();

            var name = UniqueIDToName(uniqueID);

            textComponent.text = name;
            if (name.Length > 17) textComponent.fontSize = 10;
            // Do it next frame
            var fontPath = "Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ScaleRoot/PanRoot/TH_VILLAGE/EntryCardRoot/NameBackground/Name";
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => textComponent.font = SearchUtilities.Find(fontPath).GetComponent<Text>().font);

            newCard.SetActive(true);
            newCard.transform.name = uniqueID;
            newCard.transform.localPosition = new Vector3(position.x, position.y, 40);
            newCard.transform.localRotation = Quaternion.Euler(0, 0, 0);

            var shipLogEntryCard = newCard.GetComponent<ShipLogEntryCard>();

            Texture texture = null;
            try
            {
                if (uniqueID.Equals("SolarSystem"))
                {
                    texture = ImageUtilities.GetTexture(Main.Instance, "AssetBundle/hearthian system.png");
                }
                else
                {
                    var path = $"planets/{uniqueID}.png";
                    Logger.Log($"Trying to load {path}");
                    texture = ImageUtilities.GetTexture(Main.SystemDict[uniqueID].Mod, path);
                }
            }
            catch (Exception) { }

            if (texture != null)
            {
                shipLogEntryCard._photo.sprite = MakeSprite((Texture2D)texture);
                newCard.transform.Find("EntryCardRoot/EntryCardBackground/PhotoImage").gameObject.SetActive(true);
            }

            shipLogEntryCard._hudMarkerIcon.gameObject.SetActive(false);
            shipLogEntryCard._moreToExploreIcon.gameObject.SetActive(false);
            shipLogEntryCard._unreadIcon.gameObject.SetActive(false);

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
            base.gameObject.SetActive(true);

            Locator.GetPromptManager().AddScreenPrompt(_detectiveModePrompt, _upperRightPromptList, TextAnchor.MiddleRight, -1, true);
            Locator.GetPromptManager().AddScreenPrompt(_targetSystemPrompt, _centerPromptList, TextAnchor.MiddleCenter, -1, true);
        }

        public override void ExitMode()
        {
            base.gameObject.SetActive(false);

            Locator.GetPromptManager().RemoveScreenPrompt(_detectiveModePrompt);
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
                _oneShotSource.PlayOneShot(global::AudioType.ShipLogMoveBetweenPlanets, 1f);
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

                if (_target == shipLogEntryCard) RemoveWarpTarget();
                else SetWarpTarget(shipLogEntryCard);
            }
        }

        public string UniqueIDToName(string uniqueID)
        {
            var name = TranslationHandler.GetTranslation(uniqueID, TranslationHandler.TextType.UI);

            // If it can't find a translation it just returns the key
            if (!name.Equals(uniqueID)) return name;

            // Else we return a default name
            if (uniqueID.Equals("SolarSystem")) return "Hearthian System";

            var splitString = uniqueID.Split('.');
            if (splitString.Length > 1) splitString = splitString.Skip(1).ToArray();
            name = string.Join("", splitString).SplitCamelCase();
            return name;
        }

        private int Posmod(int a, int b)
        {
            return (a % b + b) % b;
        }

        private Sprite MakeSprite(Texture2D texture)
        {
            var rect = new Rect(0, 0, texture.width, texture.height);
            var pivot = new Vector2(texture.width / 2, texture.height / 2);
            return Sprite.Create(texture, rect, pivot);
        }

        private void OnTargetReferenceFrame(ReferenceFrame referenceFrame)
        {
            RemoveWarpTarget();
        }

        private void SetWarpTarget(ShipLogEntryCard shipLogEntryCard)
        {
            RemoveWarpTarget(false);
            _oneShotSource.PlayOneShot(AudioType.ShipLogUnmarkLocation, 1f);
            _target = shipLogEntryCard;
            _target.SetMarkedOnHUD(true);
            Locator._rfTracker.UntargetReferenceFrame();
            GlobalMessenger.FireEvent("UntargetReferenceFrame");

            var name = UniqueIDToName(shipLogEntryCard.name);

            var warpNotificationDataText = TranslationHandler.GetTranslation("WARP_LOCKED", TranslationHandler.TextType.UI).Replace("{0}", name.ToUpper());
            _warpNotificationData = new NotificationData(warpNotificationDataText);
            NotificationManager.SharedInstance.PostNotification(_warpNotificationData, true);

            var warpPromptText = "<CMD> " + TranslationHandler.GetTranslation("ENGAGE_WARP_PROMPT", TranslationHandler.TextType.UI).Replace("{0}", name);
            _warpPrompt.SetText(warpPromptText);
        }

        private void RemoveWarpTarget(bool playSound = false)
        {
            if (_warpNotificationData != null) NotificationManager.SharedInstance.UnpinNotification(_warpNotificationData);
            if (_target == null) return;
            if (playSound) _oneShotSource.PlayOneShot(global::AudioType.ShipLogMarkLocation, 1f);
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
