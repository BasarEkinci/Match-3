using System.Collections.Generic;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Matching;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class MatchFinderTests
    {
        private const int Width = 8;
        private const int Height = 8;
        private const TileColor MatchColor = TileColor.Orange;

        private static readonly TileColor[] FillerColors =
        {
            TileColor.Red,
            TileColor.Green,
            TileColor.Blue,
            TileColor.Yellow,
            TileColor.Purple
        };

        private MatchFinder m_Finder;

        [SetUp]
        public void SetUp()
        {
            m_Finder = new MatchFinder(new TestBoardSettings(Width, Height));
        }

        [Test]
        public void FillerBoardHasNoMatches()
        {
            Assert.AreEqual(0, m_Finder.FindMatches(CreateFillerBoard()).Count);
        }

        [Test]
        public void HorizontalTripleIsLine3()
        {
            Board board = CreateFillerBoard();
            PaintHorizontal(board, 1, 2, 3);

            IReadOnlyList<MatchGroup> groups = m_Finder.FindMatches(board);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(MatchShape.Line3, groups[0].Shape);
            Assert.AreEqual(MatchColor, groups[0].Color);
            Assert.AreEqual(3, groups[0].Positions.Count);
        }

        [Test]
        public void HorizontalQuadIsLine4Horizontal()
        {
            Board board = CreateFillerBoard();
            PaintHorizontal(board, 1, 2, 4);

            IReadOnlyList<MatchGroup> groups = m_Finder.FindMatches(board);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(MatchShape.Line4Horizontal, groups[0].Shape);
        }

        [Test]
        public void VerticalQuadIsLine4Vertical()
        {
            Board board = CreateFillerBoard();
            PaintVertical(board, 3, 1, 4);

            IReadOnlyList<MatchGroup> groups = m_Finder.FindMatches(board);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(MatchShape.Line4Vertical, groups[0].Shape);
        }

        [Test]
        public void FiveInRowIsLine5()
        {
            Board board = CreateFillerBoard();
            PaintHorizontal(board, 1, 2, 5);

            IReadOnlyList<MatchGroup> groups = m_Finder.FindMatches(board);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(MatchShape.Line5, groups[0].Shape);
            Assert.AreEqual(5, groups[0].Positions.Count);
        }

        [Test]
        public void FiveInRowCrossedByATripleStaysLine5()
        {
            Board board = CreateFillerBoard();
            PaintHorizontal(board, 1, 2, 5);
            PaintVertical(board, 3, 2, 3);

            IReadOnlyList<MatchGroup> groups = m_Finder.FindMatches(board);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(MatchShape.Line5, groups[0].Shape);
        }

        [Test]
        public void LShapeIsSingleCornerGroup()
        {
            Board board = CreateFillerBoard();
            PaintHorizontal(board, 1, 2, 3);
            PaintVertical(board, 3, 1, 3);

            IReadOnlyList<MatchGroup> groups = m_Finder.FindMatches(board);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(MatchShape.Corner, groups[0].Shape);
            Assert.AreEqual(5, groups[0].Positions.Count);
        }

        [Test]
        public void TShapeIsSingleCornerGroup()
        {
            Board board = CreateFillerBoard();
            PaintHorizontal(board, 1, 2, 3);
            PaintVertical(board, 2, 1, 3);

            IReadOnlyList<MatchGroup> groups = m_Finder.FindMatches(board);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(MatchShape.Corner, groups[0].Shape);
            Assert.AreEqual(5, groups[0].Positions.Count);
        }

        [Test]
        public void SeparateMatchesStayDistinct()
        {
            Board board = CreateFillerBoard();
            PaintHorizontal(board, 0, 0, 3);
            PaintHorizontal(board, 5, 5, 3);

            Assert.AreEqual(2, m_Finder.FindMatches(board).Count);
        }

        [Test]
        public void EmptyCellsBreakRuns()
        {
            Board board = CreateFillerBoard();
            PaintHorizontal(board, 1, 2, 3);
            board.Set(new GridPosition(3, 2), Tile.Empty);

            Assert.AreEqual(0, m_Finder.FindMatches(board).Count);
        }

        private static Board CreateFillerBoard()
        {
            Board board = new Board(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    board.Set(new GridPosition(x, y), new Tile(FillerColors[(x + (2 * y)) % FillerColors.Length]));
                }
            }

            return board;
        }

        [Test]
        public void ColorBombDoesNotJoinMatches()
        {
            Board board = CreateFillerBoard();
            PaintHorizontal(board, 1, 2, 5);
            board.Set(new GridPosition(3, 2), new Tile(MatchColor, SpecialTileType.ColorBomb));

            IReadOnlyList<MatchGroup> groups = m_Finder.FindMatches(board);

            Assert.AreEqual(0, groups.Count);
        }

        [Test]
        public void ColorBombBreaksALongerRunIntoNothing()
        {
            Board board = CreateFillerBoard();
            PaintHorizontal(board, 1, 2, 3);
            board.Set(new GridPosition(2, 2), new Tile(MatchColor, SpecialTileType.ColorBomb));

            IReadOnlyList<MatchGroup> groups = m_Finder.FindMatches(board);

            Assert.AreEqual(0, groups.Count);
        }

        private static void PaintHorizontal(Board board, int startX, int y, int length)
        {
            for (int offset = 0; offset < length; offset++)
            {
                board.Set(new GridPosition(startX + offset, y), new Tile(MatchColor));
            }
        }

        private static void PaintVertical(Board board, int x, int startY, int length)
        {
            for (int offset = 0; offset < length; offset++)
            {
                board.Set(new GridPosition(x, startY + offset), new Tile(MatchColor));
            }
        }
    }
}
