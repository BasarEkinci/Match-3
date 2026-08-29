using System;
using System.Collections.Generic;

namespace Match3.Model.Special
{
    public sealed class ChainResolver
    {
        private readonly SpecialTileEffects m_Effects;
        private readonly List<GridPosition> m_Pending = new List<GridPosition>();

        private bool[] m_Marked;

        public ChainResolver(SpecialTileEffects effects)
        {
            m_Effects = effects;
        }

        public void Collect(Board board, IReadOnlyList<GridPosition> seeds, List<GridPosition> cleared)
        {
            PrepareMarks(board);
            cleared.Clear();
            m_Pending.Clear();

            for (int index = 0; index < seeds.Count; index++)
            {
                m_Pending.Add(seeds[index]);
            }

            for (int index = 0; index < m_Pending.Count; index++)
            {
                GridPosition position = m_Pending[index];
                int cell = (position.Y * board.Width) + position.X;
                if (m_Marked[cell])
                {
                    continue;
                }

                m_Marked[cell] = true;
                cleared.Add(position);
                m_Effects.TryCollect(board, position, m_Pending);
            }
        }

        private void PrepareMarks(Board board)
        {
            int cellCount = board.Width * board.Height;
            if (m_Marked == null || m_Marked.Length != cellCount)
            {
                m_Marked = new bool[cellCount];
                return;
            }

            Array.Clear(m_Marked, 0, m_Marked.Length);
        }
    }
}
