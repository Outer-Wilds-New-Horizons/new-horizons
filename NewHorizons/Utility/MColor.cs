using UnityEngine;
namespace NewHorizons.Utility
{
    public class MColor
    {
        public MColor(int r, int g, int b, int a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public int R { get; }
        public int G { get; }
        public int B { get; }
        public int A { get; }

        public Color32 ToColor32() => new Color32((byte)R, (byte)G, (byte)B, (byte)A);

        public Color ToColor() => new Color(R / 255f, G / 255f, B / 255f, A / 255f);
    }
}
