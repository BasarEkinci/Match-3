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

    public readonly struct RoundStartedSignal : ISignal
    {
    }

    public readonly struct RoundEndedSignal : ISignal
    {
        public RoundEndedSignal(int score)
        {
            Score = score;
        }

        public int Score { get; }
    }

    public readonly struct HighScoreChangedSignal : ISignal
    {
        public HighScoreChangedSignal(int highScore)
        {
            HighScore = highScore;
        }

        public int HighScore { get; }
    }
}
