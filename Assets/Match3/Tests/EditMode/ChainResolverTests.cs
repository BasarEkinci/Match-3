using System.Collections.Generic;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Special;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class ChainResolverTests
    {
        private const int Width = 5;
        private const int Height = 5;
        private const TileColor FillerColor = TileColor.Blue;
        private const TileColor MatchColor = TileColor.Red;
        private const int PlainMatchCellCount = 3;
        private const int RocketChainCellCount = 7;
        private const int TripleChainCellCount = 11;

        private ChainResolver m_Resolver;
        private List<GridPosition> m_Cleared;
        private Board m_Board;

        [SetUp]
        public void SetUp()
        {
            m_Resolver = new ChainResolver(new SpecialTileEffects());
            m_Cleared = new List<GridPosition>();
            m_Board = CreateFilledBoard();
        }

        [Test]
        public void PlainMatchClearsOnlyItsOwnCells()
        {
            m_Resolver.Collect(m_Board, (Row(2, 0, PlainMatchCellCount)), m_Cleared);

            Assert.AreEqual(PlainMatchCellCount, m_Cleared.Count);
        }

        [Test]
        public void RocketInsideMatchClearsItsWholeRow()
        {
            m_Board.Set(new GridPosition(1, 2), new Tile(MatchColor, SpecialTileType.HorizontalRocket));

            m_Resolver.Collect(m_Board, (Column(1, 1, PlainMatchCellCount)), m_Cleared);

            Assert.AreEqual(RocketChainCellCount, m_Cleared.Count);
            for (int x = 0; x < Width; x++)
            {
                AssertContains(new GridPosition(x, 2));
            }

            AssertContains(new GridPosition(1, 1));
            AssertContains(new GridPosition(1, 3));
        }

        [Test]
        public void RocketBombRocketChainResolvesInOnePass()
        {
            m_Board.Set(new GridPosition(2, 2), new Tile(MatchColor, SpecialTileType.HorizontalRocket));
            m_Board.Set(new GridPosition(0, 2), new Tile(MatchColor, SpecialTileType.Bomb));
            m_Board.Set(new GridPosition(0, 1), new Tile(MatchColor, SpecialTileType.VerticalRocket));

            m_Resolver.Collect(m_Board, (Single(new GridPosition(2, 2))), m_Cleared);

            for (int x = 0; x < Width; x++)
            {
                AssertContains(new GridPosition(x, 2));
            }

            for (int y = 0; y < Height; y++)
            {
                AssertContains(new GridPosition(0, y));
            }

            AssertContains(new GridPosition(1, 1));
            AssertContains(new GridPosition(1, 3));
            Assert.AreEqual(TripleChainCellCount, m_Cleared.Count);
            AssertNoDuplicates();
        }

        [Test]
        public void TwoRocketsOnTheSameRowDoNotRetrigger()
        {
            m_Board.Set(new GridPosition(0, 2), new Tile(MatchColor, SpecialTileType.HorizontalRocket));
            m_Board.Set(new GridPosition(4, 2), new Tile(MatchColor, SpecialTileType.HorizontalRocket));

            m_Resolver.Collect(m_Board, (Row(2, 0, Width)), m_Cleared);

            Assert.AreEqual(Width, m_Cleared.Count);
            AssertNoDuplicates();
        }

        [Test]
        public void ColorBombInsideMatchClearsEveryTileOfItsColour()
        {
            m_Board.Set(new GridPosition(2, 2), new Tile(MatchColor, SpecialTileType.ColorBomb));
            m_Board.Set(new GridPosition(4, 0), new Tile(MatchColor));
            m_Board.Set(new GridPosition(0, 4), new Tile(MatchColor));

            m_Resolver.Collect(m_Board, (Single(new GridPosition(2, 2))), m_Cleared);

            Assert.AreEqual(PlainMatchCellCount, m_Cleared.Count);
            AssertContains(new GridPosition(4, 0));
            AssertContains(new GridPosition(0, 4));
            AssertContains(new GridPosition(2, 2));
        }

        private void AssertContains(GridPosition position)
        {
            for (int index = 0; index < m_Cleared.Count; index++)
            {
                if (m_Cleared[index].Equals(position))
                {
                    return;
                }
            }

            Assert.Fail($"Missing cell {position.X},{position.Y}");
        }

        private void AssertNoDuplicates()
        {
            HashSet<GridPosition> seen = new HashSet<GridPosition>();
            for (int index = 0; index < m_Cleared.Count; index++)
            {
                Assert.IsTrue(seen.Add(m_Cleared[index]), "Duplicate cleared cell");
            }
        }

        private static List<GridPosition> Single(GridPosition position) =>
            new List<GridPosition> { position };

        private static List<GridPosition> Row(int y, int startX, int length)
        {
            List<GridPosition> positions = new List<GridPosition>();
            for (int offset = 0; offset < length; offset++)
            {
                positions.Add(new GridPosition(startX + offset, y));
            }

            return positions;
        }

        private static List<GridPosition> Column(int x, int startY, int length)
        {
            List<GridPosition> positions = new List<GridPosition>();
            for (int offset = 0; offset < length; offset++)
            {
                positions.Add(new GridPosition(x, startY + offset));
            }

            return positions;
        }

        private static Board CreateFilledBoard()
        {
            Board board = new Board(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    board.Set(new GridPosition(x, y), new Tile(FillerColor));
                }
            }

            return board;
        }
    }
}
