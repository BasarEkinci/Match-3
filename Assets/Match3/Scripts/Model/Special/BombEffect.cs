using System.Collections.Generic;

namespace Match3.Model.Special
{
    public sealed class BombEffect : ISpecialTileEffect
    {
        private readonly int m_Radius;

        public BombEffect(int radius)
        {
            m_Radius = radius;
        }

        public void Collect(Board board, GridPosition origin, Tile tile, List<GridPosition> cells)
        {
            for (int y = origin.Y - m_Radius; y <= origin.Y + m_Radius; y++)
            {
                for (int x = origin.X - m_Radius; x <= origin.X + m_Radius; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    if (board.TryGet(position, out Tile target) && !target.IsEmpty)
                    {
                        cells.Add(position);
                    }
                }
            }
        }
    }
}
