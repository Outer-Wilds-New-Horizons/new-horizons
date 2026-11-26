using UnityEngine;
using VisualState = NomaiTextLine.VisualState;

namespace NewHorizons.Components.Props
{
    public class NHTranslatorTextLineColorizer : MonoBehaviour
    {
        public NomaiTextLine textLine;
        public bool calculateTranslatedColor = true;
        public Color unreadColor = Color.white;
        public Color translatedColor = Color.gray;

        public void Awake()
        {
            textLine = GetComponent<NomaiTextLine>();
        }

        public void Start()
        {
            if (calculateTranslatedColor)
                translatedColor = UnreadToTranslatedColor(unreadColor);
        }

        public Color DetermineTextLineColor(VisualState state)
        {
            Color textLineColor = Color.white;
            bool hidden = true;
            if (textLine._active)
            {
                switch (state)
                {
                    case VisualState.HIDDEN:
                        break;
                    case VisualState.UNREAD:
                        hidden = false;
                        textLineColor = unreadColor;
                        break;
                    case VisualState.TRANSLATED:
                        hidden = false;
                        textLineColor = translatedColor;
                        break;
                }
            }
            textLineColor.a = hidden ? 0 : 1;
            return textLineColor;
        }

        public static Color UnreadToTranslatedColor(Color unread)
        {
            // Convert to HSV
            Color.RGBToHSV(unread, out float h, out float s, out float v);

            // Desaturate
            s *= 0.4f;

            // Darken
            v *= 0.6f;

            // Reconstruct color
            Color translated = Color.HSVToRGB(h, s, v);

            // Preserve alpha
            translated.a = unread.a;

            return translated;
        }
    }
}
