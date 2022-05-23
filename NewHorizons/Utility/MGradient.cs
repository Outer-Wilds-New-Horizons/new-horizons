namespace NewHorizons.Utility
{
    public class MGradient
    {
        public MGradient(float time, MColor tint)
        {
            Time = time;
            Tint = tint;
        }

        public float Time { get; }
        public MColor Tint { get; }
    }
}