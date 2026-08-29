using System;
using Match3.Model.Persistence;
using Match3.Model.Scoring;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;

namespace Match3.Controller
{
    public sealed class ScoreController : IDisposable
    {
        private readonly GamePipe m_GamePipe;
        private readonly ProjectPipe m_ProjectPipe;
        private readonly ScoreModel m_Score;
        private readonly ISaveRepository m_Save;

        private bool m_IsDisposed;

        public ScoreController(GamePipe gamePipe, ProjectPipe projectPipe, ScoreModel score, ISaveRepository save)
        {
            m_Save = save;
            m_GamePipe = gamePipe;
            m_ProjectPipe = projectPipe;
            m_Score = score;

            m_ProjectPipe.SubscribeTo<RoundStartedSignal>(OnRoundStarted);

            m_GamePipe.SubscribeTo<CellsClearedSignal>(OnCellsCleared);
            m_GamePipe.SubscribeTo<SpecialTileCreatedSignal>(OnSpecialTileCreated);
            m_GamePipe.SubscribeTo<SpecialCombinationTriggeredSignal>(OnSpecialCombinationTriggered);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_ProjectPipe.UnsubscribeFrom<RoundStartedSignal>(OnRoundStarted);
            m_GamePipe.UnsubscribeFrom<CellsClearedSignal>(OnCellsCleared);
            m_GamePipe.UnsubscribeFrom<SpecialTileCreatedSignal>(OnSpecialTileCreated);
            m_GamePipe.UnsubscribeFrom<SpecialCombinationTriggeredSignal>(OnSpecialCombinationTriggered);
        }

        private void OnRoundStarted(ref RoundStartedSignal signal)
        {
            m_Score.Restore(signal.IsResumed ? m_Save.LoadScore() : 0);
            RaiseChanged(0);
        }

        private void OnCellsCleared(ref CellsClearedSignal signal)
        {
            RaiseChanged(m_Score.AddClearedTiles(signal.Cells.Count, signal.CascadeStep));
        }

        private void OnSpecialTileCreated(ref SpecialTileCreatedSignal signal)
        {
            RaiseChanged(m_Score.AddSpecialTileCreation());
        }

        private void OnSpecialCombinationTriggered(ref SpecialCombinationTriggeredSignal signal)
        {
            RaiseChanged(m_Score.AddSpecialCombination());
        }

        private void RaiseChanged(int delta)
        {
            m_GamePipe.Raise(new ScoreChangedSignal(m_Score.Total, delta, m_Score.Multiplier));
        }
    }
}
