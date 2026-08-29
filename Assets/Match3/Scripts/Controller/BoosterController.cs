using System;
using Match3.Model.Boosters;
using Match3.Model.Enums;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;

namespace Match3.Controller
{
    public sealed class BoosterController : IDisposable
    {
        private readonly GamePipe m_GamePipe;
        private readonly ProjectPipe m_ProjectPipe;
        private readonly BoosterModel m_Boosters;

        private bool m_IsDisposed;

        public BoosterController(GamePipe gamePipe, ProjectPipe projectPipe, BoosterModel boosters)
        {
            m_GamePipe = gamePipe;
            m_ProjectPipe = projectPipe;
            m_Boosters = boosters;

            m_ProjectPipe.SubscribeTo<RoundStartedSignal>(OnRoundStarted);

            m_GamePipe.SubscribeTo<ScoreChangedSignal>(OnScoreChanged);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_ProjectPipe.UnsubscribeFrom<RoundStartedSignal>(OnRoundStarted);
            m_GamePipe.UnsubscribeFrom<ScoreChangedSignal>(OnScoreChanged);
        }

        private void OnRoundStarted(ref RoundStartedSignal signal)
        {
            m_Boosters.Reset();
            RaiseChargeChanged();
        }

        private void OnScoreChanged(ref ScoreChangedSignal signal)
        {
            m_Boosters.AddCharge(signal.Delta);
            while (m_Boosters.TryGrant(out BoosterType granted))
            {
                m_GamePipe.Raise(new BoosterGrantedSignal(granted));
            }

            RaiseChargeChanged();
        }

        private void RaiseChargeChanged()
        {
            m_GamePipe.Raise(new BoosterChargeChangedSignal(m_Boosters.Charge, m_Boosters.RequiredCharge));
        }
    }
}
