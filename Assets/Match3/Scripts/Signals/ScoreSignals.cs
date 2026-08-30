using Match3.Core.Signals;

namespace Match3.Signals
{
    public readonly struct ScoreChangedSignal : ISignal
    {
        public ScoreChangedSignal(int total, int delta, float multiplier)
        {
            Total = total;
            Delta = delta;
            Multiplier = multiplier;
        }

        public int Total { get; }

        public int Delta { get; }

        public float Multiplier { get; }
    }
}
