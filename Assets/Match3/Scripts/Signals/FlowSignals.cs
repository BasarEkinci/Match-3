using Match3.Model.Enums;
using Syntac.Signals;

namespace Match3.Signals
{
    public readonly struct ScreenChangeRequestedSignal : ISignal
    {
        public ScreenChangeRequestedSignal(GameScreen screen)
        {
            Screen = screen;
        }

        public GameScreen Screen { get; }
    }

    public readonly struct ScreenChangedSignal : ISignal
    {
        public ScreenChangedSignal(GameScreen screen)
        {
            Screen = screen;
        }

        public GameScreen Screen { get; }
    }

    public readonly struct RoundRestartRequestedSignal : ISignal
    {
    }

    public readonly struct RoundStartedSignal : ISignal
    {
        public RoundStartedSignal(bool isResumed)
        {
            IsResumed = isResumed;
        }

        public bool IsResumed { get; }
    }

    public readonly struct RoundEndedSignal : ISignal
    {
        public RoundEndedSignal(int score)
        {
            Score = score;
        }

        public int Score { get; }
    }
}
