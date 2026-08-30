using System;
using Match3.Model;
using Match3.Model.Persistence;
using Match3.Signals;
using Match3.Core.MessagePipe.Pipes;

namespace Match3.Controller
{
    public sealed class SaveController : IDisposable
    {
        private readonly GamePipe m_GamePipe;
        private readonly ISaveRepository m_Repository;

        private Board m_Board;
        private int m_Score;
        private bool m_IsDisposed;

        public SaveController(GamePipe gamePipe, ISaveRepository repository)
        {
            m_GamePipe = gamePipe;
            m_Repository = repository;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<ScoreChangedSignal>(OnScoreChanged);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_GamePipe.UnsubscribeFrom<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.UnsubscribeFrom<ScoreChangedSignal>(OnScoreChanged);
            m_GamePipe.UnsubscribeFrom<InputLockChangedSignal>(OnInputLockChanged);
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal)
        {
            m_Board = signal.Board;
        }

        private void OnScoreChanged(ref ScoreChangedSignal signal)
        {
            m_Score = signal.Total;
        }

        private void OnInputLockChanged(ref InputLockChangedSignal signal)
        {
            if (signal.IsLocked || m_Board == null)
            {
                return;
            }

            m_Repository.Save(m_Board, m_Score);
        }
    }
}
