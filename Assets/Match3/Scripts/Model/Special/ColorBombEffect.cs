using System.Collections.Generic;

namespace Match3.Model.Special
{
    public sealed class ColorBombEffect : ISpecialTileEffect
    {
        public void Collect(Board board, GridPosition origin, Tile tile, List<GridPosition> cells)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    if (position.Equals(origin))
                    {
                        continue;
                    }

                    board.TryGet(position, out Tile target);
                    if (!target.IsEmpty && target.Color == tile.Color)
                    {
                        cells.Add(position);
                    }
                }
            }

            cells.Add(origin);
        }
    }
}
