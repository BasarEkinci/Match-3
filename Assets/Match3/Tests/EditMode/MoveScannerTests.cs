using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Matching;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class MoveScannerTests
    {
        private const int Width = 4;
        private const int Height = 4;

        private static readonly TileColor[,] DeadlockLayout =
        {
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Green },
            { TileColor.Blue, TileColor.Green, TileColor.Blue, TileColor.Red },
            { TileColor.Red, TileColor.Red, TileColor.Blue, TileColor.Blue },
            { TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red }
        };

        private static readonly TileColor[,] PlayableLayout =
        {
            { TileColor.Green, TileColor.Green, TileColor.Blue, TileColor.Green },
            { TileColor.Green, TileColor.Blue, TileColor.Red, TileColor.Red },
            { TileColor.Blue, TileColor.Green, TileColor.Red, TileColor.Green },
            { TileColor.Red, TileColor.Green, TileColor.Green, TileColor.Red }
        };

        private MoveScanner m_Scanner;

        [SetUp]
        public void SetUp()
        {
            m_Scanner = new MoveScanner(new MatchFinder(new TestBoardSettings(Width, Height)));
        }

        [Test]
        public void DeadlockBoardHasNoMove()
        {
            Assert.IsFalse(m_Scanner.TryFindMove(CreateBoard(DeadlockLayout), out _, out _));
        }

        [Test]
        public void PlayableBoardHasMove()
        {
            Assert.IsTrue(m_Scanner.TryFindMove(CreateBoard(PlayableLayout), out _, out _));
        }

        [Test]
        public void FoundMoveCreatesMatchWhenApplied()
        {
            Board board = CreateBoard(PlayableLayout);

            Assert.IsTrue(m_Scanner.TryFindMove(board, out GridPosition from, out GridPosition to));

            board.Swap(from, to);
            Assert.Greater(new MatchFinder(new TestBoardSettings(Width, Height)).FindMatches(board).Count, 0);
        }

        [Test]
        public void ColorBombAlwaysCountsAsMove()
        {
            Board board = CreateBoard(DeadlockLayout);
            GridPosition bomb = new GridPosition(0, 0);
            board.Set(bomb, new Tile(TileColor.Red, SpecialTileType.ColorBomb));

            Assert.IsTrue(m_Scanner.TryFindMove(board, out GridPosition from, out GridPosition to));
            Assert.AreEqual(bomb, from);
            Assert.AreEqual(bomb, to);
        }

        private static Board CreateBoard(TileColor[,] layout)
        {
            Board board = new Board(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    board.Set(new GridPosition(x, y), new Tile(layout[y, x]));
                }
            }

            return board;
        }
    }
}
