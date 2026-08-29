using System.Collections.Generic;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Gravity;

namespace Match3.Tests.EditMode
{
    public sealed class StubGravityResolver : IGravityResolver
    {
        private readonly TileColor[,] m_Layout;

        public StubGravityResolver(TileColor[,] layout)
        {
            m_Layout = layout;
        }

        public void Resolve(Board board, List<TileMove> moves, List<TileSpawn> spawns)
        {
            moves.Clear();
            spawns.Clear();

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    Tile tile = new Tile(m_Layout[y, x]);
                    board.Set(position, tile);
                    spawns.Add(new TileSpawn(position, tile));
                }
            }
        }
    }
}
