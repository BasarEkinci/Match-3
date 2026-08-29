using System.Collections.Generic;

namespace Match3.Model.Special
{
    public interface ISpecialTileEffect
    {
        void Collect(Board board, GridPosition origin, Tile tile, List<GridPosition> cells);
    }
}
