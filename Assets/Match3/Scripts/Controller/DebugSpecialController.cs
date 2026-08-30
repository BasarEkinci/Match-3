#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Signals;
using Match3.Core.MessagePipe.Pipes;

namespace Match3.Controller
{
    public sealed class DebugSpecialController : IDisposable
    {
        private readonly GamePipe m_GamePipe;
        private readonly Random m_Random;
        private readonly List<GridPosition> m_Candidates = new List<GridPosition>();

        private Board m_Board;
        private bool m_IsInputLocked;
        private bool m_IsDisposed;

        public DebugSpecialController(GamePipe gamePipe, Random random)
        {
            m_GamePipe = gamePipe;
            m_Random = random;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);
            m_GamePipe.SubscribeTo<DebugSpecialRequestedSignal>(OnSpecialRequested);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_GamePipe.UnsubscribeFrom<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.UnsubscribeFrom<InputLockChangedSignal>(OnInputLockChanged);
            m_GamePipe.UnsubscribeFrom<DebugSpecialRequestedSignal>(OnSpecialRequested);
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal) => m_Board = signal.Board;

        private void OnInputLockChanged(ref InputLockChangedSignal signal) => m_IsInputLocked = signal.IsLocked;

        private void OnSpecialRequested(ref DebugSpecialRequestedSignal signal)
        {
            if (m_Board == null || m_IsInputLocked || signal.Type == SpecialTileType.None)
            {
                return;
            }

            CollectCandidates();
            if (m_Candidates.Count == 0)
            {
                return;
            }

            GridPosition position = m_Candidates[m_Random.Next(m_Candidates.Count)];
            m_Board.TryGet(position, out Tile tile);
            m_Board.Set(position, new Tile(tile.Color, signal.Type));
            m_GamePipe.Raise(new SpecialTileCreatedSignal(position, signal.Type));
        }

        private void CollectCandidates()
        {
            m_Candidates.Clear();
            for (int y = 0; y < m_Board.Height; y++)
            {
                for (int x = 0; x < m_Board.Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    m_Board.TryGet(position, out Tile tile);
                    if (tile.IsEmpty || tile.Special != SpecialTileType.None)
                    {
                        continue;
                    }

                    m_Candidates.Add(position);
                }
            }
        }
    }
}
#endif
