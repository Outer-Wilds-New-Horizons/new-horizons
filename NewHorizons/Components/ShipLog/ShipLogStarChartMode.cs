using NewHorizons.External.SerializableData;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using static NewHorizons.Utility.Files.AssetBundleUtilities;

namespace NewHorizons.Components.ShipLog
{
    public class ShipLogStarChartMode : ShipLogMode
    {
        private OWAudioSource _oneShotSource;
        private Font _fontToUse;

        private ScreenPromptList _centerPromptList;

        private ScreenPrompt _targetSystemPrompt;
        private ScreenPrompt _warpPrompt = new ScreenPrompt(InputLibrary.autopilot, "<CMD> Warp to system");

        private ShipLogStar _target = null;
        private ShipLogStar _thisStar = null;
        private ShipLogStar _switchStar = null;
        private NotificationData _warpNotificationData = null;

        private List<ShipLogStar> shipLogStars = new List<ShipLogStar>();

        public Vector2 cameraPosition;
        public float cameraZoom = 8;
        public Transform cameraPivot;
        private Transform _allStarsParent;
        private RectTransform highlightCursor;
        private RectTransform visualWarpLine;

        private Color _elementColor = new Color(245f / 255f, 158f / 255f, 44f / 255f);

        private AudioClip _onOpenClip;
        private AudioClip _onSelectClip;
        private AudioClip _onDeselectClip;
        private float _volumeScale = 0.45f;

        private Texture2D _starTexture;
        private Texture2D _cursorTexture;

        private GameObject _enableOnWarpVisuals;
        private GameObject _disableOnWarpVisuals;

        private ShipLogEntryCard _card;
        private GameObject _cardTemplate = null;

        Vector3[] _galaxyStarPoints;


        private void SetCard(string uniqueID)
        {
            _card.transform.localScale = new Vector3(1, 0, 1);

            Texture texture = null;
            try
            {
                if (uniqueID.Equals("SolarSystem"))
                {
                    texture = ImageUtilities.GetTexture(Main.Instance, "Assets/hearthian system.png");
                }
                else if (uniqueID.Equals("EyeOfTheUniverse"))
                {
                    texture = ImageUtilities.GetTexture(Main.Instance, "Assets/eye symbol.png");
                }
                else
                {
                    IModBehaviour mod = Main.SystemDict[uniqueID].Mod;

                    var path = Path.Combine("systems", uniqueID + ".png");

                    // Else check the old location
                    if (!File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, path)))
                    {
                        path = Path.Combine("planets", uniqueID + ".png");
                    }

                    texture = ImageUtilities.GetTexture(mod, path);
                }
            }
            catch (Exception) { }

            if (texture != null)
            {
                _card._questionMark.gameObject.SetActive(false);
                _card._photo.gameObject.SetActive(true);
                _card._photo.sprite = MakeSprite((Texture2D)texture);
            } else
            {
                _card._questionMark.gameObject.SetActive(true);
                _card._photo.gameObject.SetActive(false);
            }
            _card._name.text = UniqueIDToName(uniqueID);
        }


        private void CreateCard()
        {
            if (_cardTemplate == null)
            {
                var panRoot = SearchUtilities.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ScaleRoot/PanRoot");
                _cardTemplate = Instantiate(panRoot.GetComponentInChildren<ShipLogEntryCard>(true).gameObject);
                _cardTemplate.SetActive(false);
            }

            GameObject newCard = Instantiate(_cardTemplate, this.transform);
            newCard.name = "Card";
            newCard.SetActive(true);
            newCard.transform.localPosition = new Vector3(360, -150, 0);

            _card = newCard.GetAddComponent<ShipLogEntryCard>();
            _card._moreToExploreIcon.gameObject.SetActive(false);
            _card._unreadIcon.gameObject.SetActive(false);
        }

        public void InitializeStars()
        {
            _allStarsParent = new GameObject("Stars").transform;
            _allStarsParent.parent = this.transform;
            _allStarsParent.SetAsFirstSibling();
            ResetTransforms(_allStarsParent);

            RawImage highlightCursorImage = AddVisualIndicator();
            highlightCursorImage.texture = _cursorTexture;
            highlightCursor = highlightCursorImage.gameObject.GetAddComponent<RectTransform>();

            RawImage visualWarpLineImage = AddVisualIndicator();
            visualWarpLine = visualWarpLineImage.gameObject.GetAddComponent<RectTransform>();
            visualWarpLine.transform.SetAsFirstSibling();

            _fontToUse = FindObjectOfType<ShipLogController>().GetComponentInChildren<Text>().font;

            cameraPivot = new GameObject("CameraPivot").transform;
            cameraPivot.parent = this.transform;
            ResetTransforms(cameraPivot);
            cameraPivot.localEulerAngles = new Vector3(-5, 0, 0);

            foreach (Vector3 point in _galaxyStarPoints) AddStar(point);

            foreach (var starSystem in Main.SystemDict.Keys)
            {
                bool thisSystem = Main.Instance.CurrentStarSystem == starSystem;
                if (StarChartHandler.CanWarpToSystem(starSystem) || thisSystem)
                {
                    AddStar(starSystem, thisSystem);
                }
            }
        }

        private void LoadAssets()
        {
            _onOpenClip = NHPrivateAssetBundle.LoadAsset<AudioClip>("Assets/StarChart/Audio/open star map.ogg");
            _onSelectClip = NHPrivateAssetBundle.LoadAsset<AudioClip>("Assets/StarChart/Audio/select star.ogg");
            _onDeselectClip = NHPrivateAssetBundle.LoadAsset<AudioClip>("Assets/StarChart/Audio/deselect star.ogg");

            _cursorTexture = NHPrivateAssetBundle.LoadAsset<Texture2D>("Assets/StarChart/arrow.png");
            _starTexture = NHPrivateAssetBundle.LoadAsset<Texture2D>("Assets/StarChart/star.png");
        }


        private void SwitchWarpDriveVisuals(bool canWarp)
        {
            if (_enableOnWarpVisuals == null) _enableOnWarpVisuals = GameObject.Find("Ship_Body/WarpDrive/EnableOnWarp");
            if (_disableOnWarpVisuals == null) _disableOnWarpVisuals = GameObject.Find("Ship_Body/WarpDrive/DisableOnWarp");
            _enableOnWarpVisuals.SetActive(canWarp);
            _disableOnWarpVisuals.SetActive(!canWarp);
        }

        private RawImage AddVisualIndicator()
        {
            GameObject highlightCursorObject = new GameObject("VisualIndicator");
            highlightCursorObject.transform.parent = this.transform;
            ResetTransforms(highlightCursorObject.transform);
            RawImage image = highlightCursorObject.AddComponent<RawImage>();
            image.color = _elementColor;
            return image;
        }



        private void AddStar()
        {
            AddStar("", false);
        }

        internal void AddStar(string customName)
        {
            AddStar(customName, Main.Instance.CurrentStarSystem == customName);
        }

        private void AddStar(string customName, bool isThisSystem)
        {
            AddStar(customName, isThisSystem, false, Vector3.zero);
        }

        private void AddStar(Vector3 customPosition)
        {
            AddStar("", false, true, customPosition);
        }

        private void AddStar(string customName, bool isThisSystem, bool hasInputPosition,  Vector3 inputPosition)
        {
            GameObject newStarObject = new GameObject("Star");
            ShipLogStar newStar = newStarObject.AddComponent<ShipLogStar>();
            newStar.transform.parent = _allStarsParent;

            RawImage starImage = newStarObject.AddComponent<RawImage>();
            starImage.texture = _starTexture;
            newStarObject.AddComponent<CanvasRenderer>();
            ResetTransforms(newStar.transform);

            if (!string.IsNullOrEmpty(customName))
            {
                // The seed for any default (random) fields of a custom star is based on the hash of their unique name (plus ten, bc why not).
                UnityEngine.Random.InitState(customName.GetHashCode() + 10);


                External.Configs.StarSystemConfig.StarChartModule config = Main.SystemDict[customName].Config.StarChart;

                //Null check config on each so that we don't have to repeat each else statement
                //(Basically if we null check the config only once, then we have to provide the "else" statement both when the field is null and when the whole config is null)
                if (config != null && config.position != null)
                {
                    newStar._starPosition = new Vector3(config.position.x, config.position.y, 0);
                } else
                {
                    newStar._starPosition = new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f), 0);
                }

                if (config != null && config.color != null)
                {
                    starImage.color = new Color(config.color.r, config.color.g, config.color.b, config.color.a);
                }
                else
                {
                    starImage.color = Color.white;
                }

                if (config != null && config.starTexturePath != null)
                {
                    IModBehaviour mod = Main.SystemDict[customName].Mod;
                    string path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, config.starTexturePath);
                    if (File.Exists(path)) starImage.texture = ImageUtilities.GetTexture(mod, path);
                }

                if (config != null)
                {
                    newStar._starTimeLoopEnd = config.disappearanceTime;
                } else
                {
                    newStar._starTimeLoopEnd = 30;
                }

                newStar._starScale = 0.6f;
                newStar._starName = customName;

                newStarObject.name = customName;
                if (!isThisSystem)
                {
                    shipLogStars.Add(newStar); 
                } else
                {
                    _thisStar = newStar;
                    cameraPosition = new Vector2(newStar._starPosition.x, newStar._starPosition.y);
                }

                AddTextLabel(newStarObject.transform, UniqueIDToName(customName));
            } else
            {
                starImage.color = RandomStarColor();
                newStar._starTimeLoopEnd = UnityEngine.Random.Range(0f, 23f);
                newStar._starScale = UnityEngine.Random.Range(0.03f, 0.2f);
                newStar._starPosition = new Vector3(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-5f, 5f));
            }

            if (hasInputPosition)
            {
                newStar._starPosition = inputPosition;
            }

            newStar.enabled = true;

            newStar.Initialize(this);
        }

        private Color RandomStarColor()
        {
            Color blueYellowColor = Color.Lerp(Color.blue, Color.yellow, UnityEngine.Random.Range(0f, 1f));
            Color darkLightColor = Color.Lerp(blueYellowColor, Color.white, UnityEngine.Random.Range(0.8f, 1f));
            return darkLightColor;
        }

        private void AddTextLabel(Transform parent, string Text)
        {
            GameObject textObject = new GameObject("TextLabel");
            textObject.transform.parent = parent;
            ResetTransforms(textObject.transform);
            textObject.transform.localScale = Vector3.one * 2;

            Text text = textObject.AddComponent<Text>();
            RectTransform rect = textObject.GetAddComponent<RectTransform>();
            textObject.AddComponent<CanvasRenderer>();

            text.alignment = TextAnchor.UpperCenter;
            text.text = Text;
            text.font = _fontToUse;
        }

        private void ResetTransforms(Transform t)
        {
            t.transform.localPosition = Vector3.zero;
            t.transform.localScale = Vector3.one;
            t.transform.localRotation = Quaternion.identity;
        }


        private void Awake()
        {
            // Prompts
            Locator.GetPromptManager().AddScreenPrompt(_warpPrompt, PromptPosition.UpperLeft, false);
        }

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            _galaxyStarPoints = CreateGalaxy();
            CreateCard();
            LoadAssets();
            InitializeStars();
            _oneShotSource = oneShotSource;
            _centerPromptList = centerPromptList;
            _targetSystemPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, TranslationHandler.GetTranslation("LOCK_AUTOPILOT_WARP", TranslationHandler.TextType.UI), 0, ScreenPrompt.DisplayState.Normal, false);
            GlobalMessenger<ReferenceFrame>.AddListener("TargetReferenceFrame", new Callback<ReferenceFrame>(OnTargetReferenceFrame));
        }

        public void OnDestroy()
        {
            GlobalMessenger<ReferenceFrame>.RemoveListener("TargetReferenceFrame", new Callback<ReferenceFrame>(OnTargetReferenceFrame));

            Locator.GetPromptManager()?.RemoveScreenPrompt(_warpPrompt, PromptPosition.UpperLeft);
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
            Locator.GetPromptManager().AddScreenPrompt(_targetSystemPrompt, _centerPromptList, TextAnchor.MiddleCenter, -1, false);
        }

        public override void ExitMode()
        {
            gameObject.SetActive(false);
            cameraPosition = new Vector2(_thisStar._starPosition.x, _thisStar._starPosition.y);
            cameraZoom = 8;
            cameraPivot.localEulerAngles = new Vector3(-5, 0, 0);
            cameraPivot.localScale = Vector3.zero;

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
            UpdateSelection();
            UpdatePrompts();
        }

        private void UpdateMapCamera()
        {
            cameraPivot.transform.localScale = Vector3.Lerp(cameraPivot.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 2);
            if (_target != null && _card != null)
            {
                _card.transform.localScale = Vector3.Lerp(_card.transform.localScale, Vector3.one * 1.25f, Time.unscaledDeltaTime * 20);
            } else
            {
                _card.transform.localScale = Vector3.Lerp(_card.transform.localScale, Vector3.zero, Time.unscaledDeltaTime * 20);
            }

            float zoom = OWInput.GetValue(InputLibrary.mapZoomIn) - OWInput.GetValue(InputLibrary.mapZoomOut);
            cameraZoom *= 1 + (zoom * Time.unscaledDeltaTime * 2);
            cameraZoom = Mathf.Clamp(cameraZoom, 1f, 10f);

            Vector2 axisValue = OWInput.GetAxisValue(InputLibrary.moveXZ);
            cameraPosition += (axisValue * Time.unscaledDeltaTime * 500) / cameraZoom;

            _card._nameBackground.color = _elementColor;
            _card._border.color = _elementColor;
            _card._hudMarkerIcon.gameObject.SetActive(true);
        }

        private void UpdateSelection()
        {
            float minimumDistance = Mathf.Infinity;
            ShipLogStar highlightedStar = null;
            foreach(ShipLogStar s in shipLogStars)
            {
                float distance = Vector3.Distance(s.transform.localPosition, Vector3.zero);
                if (distance < minimumDistance && distance < 200 && s.gameObject.activeSelf)
                {
                    minimumDistance = distance;
                    highlightedStar = s;
                }
            }

            if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD, InputMode.All))
            {
                if (highlightedStar != null && _target != highlightedStar)
                {
                    SetWarpTarget(highlightedStar);
                } else
                {
                    RemoveWarpTarget(true);
                }
            }


            if (highlightCursor != null)
            {
                if (highlightedStar != null && _target != highlightedStar)
                {
                    _targetSystemPrompt.SetVisibility(true);
                    highlightCursor.localPosition = highlightedStar.transform.localPosition + (Vector3.down * 30);
                    highlightCursor.localScale = Vector3.one * 0.25f;

                    if (_switchStar != highlightedStar) _oneShotSource.PlayOneShot(AudioType.ShipLogHighlightEntry);

                    _switchStar = highlightedStar;
                } else
                {
                    _targetSystemPrompt.SetVisibility(false);
                    highlightCursor.localScale = Vector3.zero;
                    _switchStar = null;
                }
            } else
            {
                _switchStar = null;
            }

            if (visualWarpLine != null)
            {
                if (_target != null)
                {
                    SetWarpLinePositions(_thisStar.transform.localPosition, _target.transform.localPosition);
                } else
                {
                    SetWarpLinePositions(_thisStar.transform.localPosition, Vector3.zero);
                }
            }

            if (_target != null && !_target.gameObject.activeSelf) RemoveWarpTarget(false);
        }

        private void SetWarpLinePositions(Vector3 pos1, Vector3 pos2)
        {
            visualWarpLine.localPosition = Vector3.Lerp(pos1, pos2, 0.5f);
            visualWarpLine.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(pos1.x - pos2.x, pos1.y - pos2.y) * (180 / Mathf.PI) * -1);
            visualWarpLine.localScale = new Vector3(0.01f, Vector3.Distance(pos1, pos2) * 0.01f, 1f);
        }

        private void UpdatePrompts()
        {
        }

        public static string UniqueIDToName(string uniqueID)
        {
            var name = TranslationHandler.GetTranslation(uniqueID, TranslationHandler.TextType.UI);

            // If it can't find a translation it just returns the key
            if (!name.Equals(uniqueID)) return name;

            // Else we return a default name
            if (uniqueID.Equals("SolarSystem")) return "The Outer Wilds";

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
            return Sprite.Create(texture, rect, pivot, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);
        }

        private void OnTargetReferenceFrame(ReferenceFrame referenceFrame)
        {
            RemoveWarpTarget();
        }

        private void SetWarpTarget(ShipLogStar starTarget)
        {
            RemoveWarpTarget(false);
            if (starTarget != null)
            {
                SetCard(starTarget._starName);
            }
            _oneShotSource.PlayOneShot(_onSelectClip, _volumeScale);
            _target = starTarget;
            Locator._rfTracker.UntargetReferenceFrame();
            GlobalMessenger.FireEvent("UntargetReferenceFrame");

            var name = UniqueIDToName(starTarget.name);

            var warpNotificationDataText = TranslationHandler.GetTranslation("WARP_LOCKED", TranslationHandler.TextType.UI).Replace("{0}", name.ToUpperFixed());
            _warpNotificationData = new NotificationData(warpNotificationDataText);
            NotificationManager.SharedInstance.PostNotification(_warpNotificationData, true);

            var warpPromptText = "<CMD> " + TranslationHandler.GetTranslation("ENGAGE_WARP_PROMPT", TranslationHandler.TextType.UI).Replace("{0}", name);
            _warpPrompt.SetText(warpPromptText);
            SwitchWarpDriveVisuals(true);
        }

        private void RemoveWarpTarget(bool playSound = false)
        {
            if (_warpNotificationData != null) NotificationManager.SharedInstance.UnpinNotification(_warpNotificationData);
            if (_target == null) return;
            if (playSound) _oneShotSource.PlayOneShot(_onDeselectClip, _volumeScale);
            _target = null;
            SwitchWarpDriveVisuals(false);
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




        // Galaxy Visuals
        private Vector3[] CreateGalaxy()
        {
            List<Vector3> galaxyStarPoints = new List<Vector3>();

            UnityEngine.Random.InitState(1);
            int arms = 7;
            for (int i = 0; i < arms; i++)
            {
               CreateGalaxyArm(
                   galaxyStarPoints,
                   360*((float)i/(float)arms),
                   12, 1,
                   60, 200,
                   UnityEngine.Random.Range(2f, 3f),
                   24
                   );
            }

            return galaxyStarPoints.ToArray();
        }

        private void CreateGalaxyArm(List<Vector3> list, float startingAngle, float startDensity, float endDensity, float startScatter, float endScatter, float armLength, int bunchCount)
        {
            Vector3 moveDirection = Quaternion.AngleAxis(startingAngle, Vector3.forward) * Vector3.up;
            Vector3 point = moveDirection * 5;
            float RotateDirection = 10f;
            for (int i = 0; i < bunchCount; i++)
            {
                moveDirection = Quaternion.AngleAxis(RotateDirection, Vector3.forward) * moveDirection;

                float t = (float)i / (float)bunchCount;
                float scatter = Mathf.Lerp(startScatter, endScatter, t);
                float density = Mathf.Lerp(startDensity, endDensity, t);

                for (int s = 0; s < (int)density; s++)
                {
                    Vector3 scatterOffset = RandomStarOffsetVector(scatter);
                    list.Add(point + scatterOffset);
                }

                point += moveDirection * 30 * armLength;
            }
        }
        private Vector3 RandomStarOffsetVector(float Range)
        {
            return new Vector3(RandomStarRange(Range), RandomStarRange(Range), RandomStarRange(Range) * 0.03f);
        }
        private float RandomStarRange(float Range)
        {
            return UnityEngine.Random.Range(Range * -1, Range);
        }
    }
}
