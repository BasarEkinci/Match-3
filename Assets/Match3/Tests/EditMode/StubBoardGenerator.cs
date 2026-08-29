using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Generation;

namespace Match3.Tests.EditMode
{
    public sealed class StubBoardGenerator : IBoardGenerator
    {
        private readonly TileColor[,] m_Layout;

        public StubBoardGenerator(TileColor[,] layout)
        {
            m_Layout = layout;
        }

        public void Generate(Board board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board.Set(new GridPosition(x, y), new Tile(m_Layout[y, x]));
                }
            }
        }
    }
}
