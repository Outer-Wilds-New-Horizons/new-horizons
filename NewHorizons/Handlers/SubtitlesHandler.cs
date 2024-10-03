using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NewHorizons.Handlers
{
    class SubtitlesHandler : MonoBehaviour
    {
        public static float SUBTITLE_HEIGHT = 97;
        public static float SUBTITLE_WIDTH = 669; // nice

        public float fadeSpeed = 0.005f;
        public float fade = 1;
        public bool fadingAway = true;

        public List<Sprite> possibleSubtitles = new List<Sprite>();
        public bool eoteSubtitleHasBeenInserted = false;
        public Sprite eoteSprite;
        public int subtitleIndex;

        public static readonly int PAUSE_TIMER_MAX = 50;
        public int pauseTimer = PAUSE_TIMER_MAX;

        private Image _subtitleDisplay;
        private Graphic _graphic;

        private static List<(IModBehaviour mod, string filePath)> _additionalSubtitles = new();

        private CanvasGroup _titleCanvasGroup;

        public static void RegisterAdditionalSubtitle(IModBehaviour mod, string filePath)
        {
            _additionalSubtitles.Add((mod, filePath));
        }

        public void CheckForEOTE()
        {
            if (!eoteSubtitleHasBeenInserted)
            {
                if (Main.HasDLC)
                {
                    if (eoteSprite != null)
                    {
                        // Don't make it appear first actually because we have mods to display!
                        possibleSubtitles.Add(eoteSprite);
                    }
                    eoteSubtitleHasBeenInserted = true;
                }
            }
        }

        public void Start()
        {
            // We preserve the current image to add it to our custom subtitle
            // We also need this element to preserve its size
            GetComponent<CanvasGroup>().alpha = 1;
            var image = GetComponent<Image>();
            eoteSprite = image.sprite;
            image.sprite = null;
            image.enabled = false;
            var layout = GetComponent<LayoutElement>();
            layout.minHeight = SUBTITLE_HEIGHT;

            _titleCanvasGroup = SearchUtilities.Find("TitleCanvas").GetComponent<CanvasGroup>();

            CheckForEOTE();

            // We add our subtitles as a child object so that their sizing doesnt shift the layout of the main menu
            _subtitleDisplay = new GameObject("SubtitleDisplay").AddComponent<Image>();
            _subtitleDisplay.transform.parent = transform;
            _subtitleDisplay.transform.localPosition = new Vector3(0, 0, 0);
            _subtitleDisplay.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            _graphic = _subtitleDisplay.gameObject.GetAddComponent<Graphic>();
            _subtitleDisplay.gameObject.GetAddComponent<LayoutElement>().minWidth = SUBTITLE_WIDTH;

            AddSubtitles();
        }

        private void AddSubtitles()
        {
            foreach (var mod in Main.MountedAddons)
            {
                if (Main.AddonConfigs.TryGetValue(mod, out var addonConfig) && File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, addonConfig.subtitlePath)))
                {
                    AddSubtitle(mod, addonConfig.subtitlePath);
                }
                // Else default to subtitle.png
                else if (File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, "subtitle.png")))
                {
                    AddSubtitle(mod, "subtitle.png");
                }
            }
            foreach (var pair in _additionalSubtitles)
            {
                AddSubtitle(pair.mod, pair.filePath);
            }
        }

        public void AddSubtitle(IModBehaviour mod, string filepath)
        {
            NHLogger.Log($"Adding subtitle for {mod.ModHelper.Manifest.Name}");

            var tex = ImageUtilities.GetTexture(mod, filepath, false);
            if (tex == null) return;

            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, Mathf.Max(SUBTITLE_HEIGHT, tex.height)), new Vector2(0.5f, 0.5f), 100.0f);
            AddSubtitle(sprite);
        }

        public void AddSubtitle(Sprite sprite)
        {
            possibleSubtitles.Add(sprite);
        }

        public void FixedUpdate()
        {
            CheckForEOTE();

            if (possibleSubtitles.Count == 0)
            {
                return;
            }

            _subtitleDisplay.transform.localPosition = new Vector3(0, -36, 0);

            if (_subtitleDisplay.sprite == null)
            {
                _subtitleDisplay.sprite = possibleSubtitles[0];
                // Always call this in case we stop changing subtitles after
                ChangeSubtitle();
            }

            // don't fade transition subtitles if there's only one subtitle
            if (possibleSubtitles.Count <= 1)
            {
                return;
            }

            // Fix subtitles start cycling before the main menu is visible #844
            if (_titleCanvasGroup.alpha < 1)
            {
                return;
            }

            if (pauseTimer > 0)
            {
                pauseTimer--;
                return;
            }

            if (fadingAway)
            {
                fade -= fadeSpeed;

                if (fade <= 0)
                {
                    fade = 0;
                    ChangeSubtitle();
                    fadingAway = false;
                }
            }
            else
            {
                fade += fadeSpeed;

                if (fade >= 1)
                {
                    fade = 1;
                    fadingAway = true;
                    pauseTimer = PAUSE_TIMER_MAX;
                }
            }

            _graphic.color = new Color(1, 1, 1, fade);
        }

        public void ChangeSubtitle()
        {
            subtitleIndex = (subtitleIndex + 1) % possibleSubtitles.Count;

            var subtitle = possibleSubtitles[subtitleIndex];
            _subtitleDisplay.sprite = subtitle;
            var width = subtitle.texture.width;
            var height = subtitle.texture.height;
            var ratio = SUBTITLE_WIDTH / width; // one of these needs to be a float so that compiler doesn't think "oh 2 integers! let's round to nearest whole"
            _subtitleDisplay.rectTransform.sizeDelta = new Vector2(width, height) * ratio;
        }
    }
}
