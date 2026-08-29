using System.Collections.Generic;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Special;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class SpecialCombinationResolverTests
    {
        private const int Width = 7;
        private const int Height = 7;
        private const TileColor FillerColor = TileColor.Blue;
        private const TileColor PartnerColor = TileColor.Red;
        private const int CrossCellCount = 13;
        private const int TripleCrossCellCount = 33;
        private const int WideBlastCellCount = 25;
        private const int PartnerCellCount = 3;

        private static readonly GridPosition From = new GridPosition(3, 3);
        private static readonly GridPosition To = new GridPosition(4, 3);

        private SpecialCombinationResolver m_Resolver;
        private List<GridPosition> m_Cells;
        private Board m_Board;

        [SetUp]
        public void SetUp()
        {
            m_Resolver = new SpecialCombinationResolver();
            m_Cells = new List<GridPosition>();
            m_Board = CreateFilledBoard();
        }

        [Test]
        public void PlainTilesAreNotACombination()
        {
            Assert.IsFalse(m_Resolver.Contains(SpecialTileType.None, SpecialTileType.None));
            Assert.IsFalse(m_Resolver.Contains(SpecialTileType.Bomb, SpecialTileType.None));
            Assert.IsFalse(m_Resolver.TryResolve(m_Board, From, To, m_Cells));
        }

        [Test]
        public void RocketAndRocketClearsACross()
        {
            Place(SpecialTileType.HorizontalRocket, SpecialTileType.VerticalRocket);

            Assert.IsTrue(m_Resolver.TryResolve(m_Board, From, To, m_Cells));

            Assert.AreEqual(CrossCellCount, UniqueCount());
            for (int x = 0; x < Width; x++)
            {
                AssertContains(new GridPosition(x, To.Y));
            }

            for (int y = 0; y < Height; y++)
            {
                AssertContains(new GridPosition(To.X, y));
            }
        }

        [Test]
        public void RocketAndBombClearsThreeRowsAndThreeColumns()
        {
            Place(SpecialTileType.HorizontalRocket, SpecialTileType.Bomb);

            Assert.IsTrue(m_Resolver.TryResolve(m_Board, From, To, m_Cells));

            Assert.AreEqual(TripleCrossCellCount, UniqueCount());
            for (int offset = -1; offset <= 1; offset++)
            {
                for (int x = 0; x < Width; x++)
                {
                    AssertContains(new GridPosition(x, To.Y + offset));
                }

                for (int y = 0; y < Height; y++)
                {
                    AssertContains(new GridPosition(To.X + offset, y));
                }
            }
        }

        [Test]
        public void BombAndBombClearsFiveByFive()
        {
            Place(SpecialTileType.Bomb, SpecialTileType.Bomb);

            Assert.IsTrue(m_Resolver.TryResolve(m_Board, From, To, m_Cells));

            Assert.AreEqual(WideBlastCellCount, UniqueCount());
            AssertContains(new GridPosition(To.X - 2, To.Y - 2));
            AssertContains(new GridPosition(To.X + 2, To.Y + 2));
        }

        [Test]
        public void ColorBombAndRocketFiresARocketOnEveryPartnerColourTile()
        {
            Place(SpecialTileType.ColorBomb, SpecialTileType.HorizontalRocket);
            m_Board.Set(new GridPosition(0, 0), new Tile(PartnerColor));
            m_Board.Set(new GridPosition(6, 6), new Tile(PartnerColor));

            Assert.IsTrue(m_Resolver.TryResolve(m_Board, From, To, m_Cells));

            for (int x = 0; x < Width; x++)
            {
                AssertContains(new GridPosition(x, 0));
                AssertContains(new GridPosition(x, 6));
            }
        }

        [Test]
        public void ColorBombAndBombDetonatesEveryPartnerColourTile()
        {
            Place(SpecialTileType.ColorBomb, SpecialTileType.Bomb);
            m_Board.Set(new GridPosition(0, 0), new Tile(PartnerColor));

            Assert.IsTrue(m_Resolver.TryResolve(m_Board, From, To, m_Cells));

            AssertContains(new GridPosition(0, 0));
            AssertContains(new GridPosition(1, 0));
            AssertContains(new GridPosition(0, 1));
            AssertContains(new GridPosition(1, 1));
        }

        [Test]
        public void ColorBombAndColorBombClearsTheWholeBoard()
        {
            Place(SpecialTileType.ColorBomb, SpecialTileType.ColorBomb);

            Assert.IsTrue(m_Resolver.TryResolve(m_Board, From, To, m_Cells));

            Assert.AreEqual(Width * Height, UniqueCount());
        }

        private void Place(SpecialTileType first, SpecialTileType second)
        {
            m_Board.Set(From, new Tile(PartnerColor, first));
            m_Board.Set(To, new Tile(PartnerColor, second));
        }

        private int UniqueCount()
        {
            HashSet<GridPosition> unique = new HashSet<GridPosition>();
            for (int index = 0; index < m_Cells.Count; index++)
            {
                unique.Add(m_Cells[index]);
            }

            return unique.Count;
        }

        private void AssertContains(GridPosition position)
        {
            for (int index = 0; index < m_Cells.Count; index++)
            {
                if (m_Cells[index].Equals(position))
                {
                    return;
                }
            }

            Assert.Fail($"Missing cell {position.X},{position.Y}");
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
