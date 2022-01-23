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

        public Color ToColor() => new Color(R / 255, G / 255, B / 255, A / 255);
    }
}
