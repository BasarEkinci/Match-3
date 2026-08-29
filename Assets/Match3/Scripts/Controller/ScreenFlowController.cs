using System;
using Match3.Model.Enums;
using Match3.Model.Persistence;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;

namespace Match3.Controller
{
    public sealed class ScreenFlowController : IDisposable
    {
        private readonly GamePipe m_GamePipe;
        private readonly ProjectPipe m_ProjectPipe;
        private readonly ISaveRepository m_Save;

        private GameScreen m_Current;
        private int m_Score;
        private bool m_HasStartedRound;
        private bool m_IsDisposed;

        public ScreenFlowController(GamePipe gamePipe, ProjectPipe projectPipe, ISaveRepository save)
        {
            m_GamePipe = gamePipe;
            m_ProjectPipe = projectPipe;
            m_Save = save;
            m_Current = GameScreen.Main;

            m_ProjectPipe.SubscribeTo<ScreenChangeRequestedSignal>(OnScreenChangeRequested);
            m_ProjectPipe.SubscribeTo<RoundRestartRequestedSignal>(OnRoundRestartRequested);
            m_GamePipe.SubscribeTo<ScoreChangedSignal>(OnScoreChanged);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_ProjectPipe.UnsubscribeFrom<ScreenChangeRequestedSignal>(OnScreenChangeRequested);
            m_ProjectPipe.UnsubscribeFrom<RoundRestartRequestedSignal>(OnRoundRestartRequested);
            m_GamePipe.UnsubscribeFrom<ScoreChangedSignal>(OnScoreChanged);
        }

        private void OnScoreChanged(ref ScoreChangedSignal signal)
        {
            m_Score = signal.Total;
        }

        private void OnRoundRestartRequested(ref RoundRestartRequestedSignal signal)
        {
            if (m_Current != GameScreen.Game && m_Current != GameScreen.Pause)
            {
                return;
            }

            StartRound();
        }

        private void OnScreenChangeRequested(ref ScreenChangeRequestedSignal signal)
        {
            if (!IsAllowed(m_Current, signal.Screen))
            {
                return;
            }

            if (signal.Screen == GameScreen.RoundEnd)
            {
                m_ProjectPipe.Raise(new RoundEndedSignal(m_Score));
            }

            bool isResuming = m_Current == GameScreen.Pause;
            m_Current = signal.Screen;
            m_ProjectPipe.Raise(new ScreenChangedSignal(m_Current));

            if (m_Current == GameScreen.Game && !isResuming)
            {
                RaiseRoundStarted();
            }
        }

        private void StartRound()
        {
            m_Current = GameScreen.Game;
            m_ProjectPipe.Raise(new ScreenChangedSignal(m_Current));
            RaiseRoundStarted();
        }

        private void RaiseRoundStarted()
        {
            bool isResumed = !m_HasStartedRound && m_Save.HasSave;
            m_HasStartedRound = true;
            m_ProjectPipe.Raise(new RoundStartedSignal(isResumed));
        }

        private static bool IsAllowed(GameScreen current, GameScreen next)
        {
            switch (current)
            {
                case GameScreen.Main:
                    return next == GameScreen.Game;
                case GameScreen.Game:
                    return next == GameScreen.Pause || next == GameScreen.RoundEnd;
                case GameScreen.Pause:
                    return next == GameScreen.Game || next == GameScreen.Main || next == GameScreen.RoundEnd;
                default:
                    return next == GameScreen.Game || next == GameScreen.Main;
            }
        }
    }
}
