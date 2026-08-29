using System.Collections.Generic;

namespace Match3.Model.Special
{
    public sealed class RocketEffect : ISpecialTileEffect
    {
        private readonly bool m_Horizontal;

        public RocketEffect(bool horizontal)
        {
            m_Horizontal = horizontal;
        }

        public void Collect(Board board, GridPosition origin, Tile tile, List<GridPosition> cells)
        {
            int length = m_Horizontal ? board.Width : board.Height;
            for (int offset = 0; offset < length; offset++)
            {
                GridPosition position = m_Horizontal
                    ? new GridPosition(offset, origin.Y)
                    : new GridPosition(origin.X, offset);
                if (board.TryGet(position, out Tile target) && !target.IsEmpty)
                {
                    cells.Add(position);
                }
            }
        }
    }
}
