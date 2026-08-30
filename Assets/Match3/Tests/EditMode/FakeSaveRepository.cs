using Match3.Model;
using Match3.Model.Persistence;

namespace Match3.Tests.EditMode
{
    public sealed class FakeSaveRepository : ISaveRepository
    {
        private Tile[] m_Cells;
        private int m_Width;
        private int m_Height;

        public bool HasSave => m_Cells != null;

        public int SaveCount { get; private set; }

        public int Score { get; private set; }

        public void LoadBoard(Board board)
        {
            for (int y = 0; y < m_Height; y++)
            {
                for (int x = 0; x < m_Width; x++)
                {
                    board.Set(new GridPosition(x, y), m_Cells[(y * m_Width) + x]);
                }
            }
        }

        public int LoadScore() => Score;

        public void Save(Board board, int score)
        {
            SaveCount++;
            Score = score;
            m_Width = board.Width;
            m_Height = board.Height;
            m_Cells = new Tile[board.Width * board.Height];
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board.TryGet(new GridPosition(x, y), out Tile tile);
                    m_Cells[(y * board.Width) + x] = tile;
                }
            }
        }
    }
}
