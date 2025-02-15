using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
namespace NewHorizons.External.SerializableData
{
    [JsonObject]
    public class MColor
    {
        public MColor(int r, int g, int b, int a = 255)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static MColor FromColor(Color color)
        {
            return new MColor((int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255), (int)(color.a * 255));
        }

        /// <summary>
        /// The red component of this colour from 0-255, higher values will make the colour glow if applicable.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
        public int r;

        /// <summary>
        /// The green component of this colour from 0-255, higher values will make the colour glow if applicable.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
        public int g;

        /// <summary>
        /// The blue component of this colour from 0-255, higher values will make the colour glow if applicable.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
        public int b;

        /// <summary>
        /// The alpha (opacity) component of this colour
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0, 255)]
        [DefaultValue(255)]
        public int a = 255;

        public Color ToColor() => new Color(r / 255f, g / 255f, b / 255f, a / 255f);

        public static MColor red => new MColor(255, 0, 0);

        public static MColor green => new MColor(0, 255, 0);

        public static MColor blue => new MColor(0, 0, 255);

        public static MColor white => new MColor(255, 255, 255);

        public static MColor black => new MColor(0, 0, 0);

        public static MColor yellow => new MColor(255, 235, 4);

        public static MColor cyan => new MColor(0, 255, 255);

        public static MColor magenta => new MColor(255, 0, 255);

        public static MColor gray => new MColor(127, 127, 127);

        public static MColor grey => new MColor(127, 127, 127);

        public static MColor clear => new MColor(0, 0, 0, 0);
    }
}
