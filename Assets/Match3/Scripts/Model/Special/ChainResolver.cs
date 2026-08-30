using System;
using System.Collections.Generic;

namespace Match3.Model.Special
{
    public sealed class ChainResolver
    {
        private readonly SpecialTileEffects m_Effects;
        private readonly List<ClearedCell> m_Pending = new List<ClearedCell>();
        private readonly List<GridPosition> m_Buffer = new List<GridPosition>();

        private bool[] m_Marked;

        public ChainResolver(SpecialTileEffects effects)
        {
            m_Effects = effects;
        }

        public void Collect(Board board, IReadOnlyList<ClearedCell> seeds, List<ClearedCell> cleared)
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
                ClearedCell source = m_Pending[index];
                int cell = (source.Position.Y * board.Width) + source.Position.X;
                if (m_Marked[cell])
                {
                    continue;
                }

                m_Marked[cell] = true;
                cleared.Add(source);
                Expand(board, source);
            }
        }

        private void Expand(Board board, ClearedCell source)
        {
            m_Buffer.Clear();
            if (!m_Effects.TryCollect(board, source.Position, m_Buffer))
            {
                return;
            }

            for (int index = 0; index < m_Buffer.Count; index++)
            {
                GridPosition position = m_Buffer[index];
                m_Pending.Add(
                    new ClearedCell(position, source.Wave + GridPosition.Distance(source.Position, position)));
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
