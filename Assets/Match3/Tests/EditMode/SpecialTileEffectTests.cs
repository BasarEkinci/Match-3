using System.Collections.Generic;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Special;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class SpecialTileEffectTests
    {
        private const int Width = 5;
        private const int Height = 5;
        private const TileColor FillerColor = TileColor.Blue;
        private const TileColor TargetColor = TileColor.Red;
        private const int BombCellCount = 9;
        private const int ClippedBombCellCount = 4;
        private const int TargetColorCellCount = 3;
        private const int BombRadius = 1;

        private SpecialTileEffects m_Effects;
        private List<GridPosition> m_Cells;

        [SetUp]
        public void SetUp()
        {
            m_Effects = new SpecialTileEffects();
            m_Cells = new List<GridPosition>();
        }

        [Test]
        public void PlainTileHasNoEffect()
        {
            Board board = CreateFilledBoard();

            Assert.IsFalse(m_Effects.TryCollect(board, new GridPosition(2, 2), m_Cells));
            Assert.AreEqual(0, m_Cells.Count);
        }

        [Test]
        public void EmptyCellHasNoEffect()
        {
            Board board = CreateFilledBoard();
            board.Set(new GridPosition(2, 2), Tile.Empty);

            Assert.IsFalse(m_Effects.TryCollect(board, new GridPosition(2, 2), m_Cells));
        }

        [Test]
        public void HorizontalRocketClearsItsRow()
        {
            Board board = CreateFilledBoard();
            PlaceSpecial(board, new GridPosition(2, 3), SpecialTileType.HorizontalRocket);

            Assert.IsTrue(m_Effects.TryCollect(board, new GridPosition(2, 3), m_Cells));

            Assert.AreEqual(Width, m_Cells.Count);
            for (int x = 0; x < Width; x++)
            {
                AssertContains(new GridPosition(x, 3));
            }
        }

        [Test]
        public void VerticalRocketClearsItsColumn()
        {
            Board board = CreateFilledBoard();
            PlaceSpecial(board, new GridPosition(2, 3), SpecialTileType.VerticalRocket);

            Assert.IsTrue(m_Effects.TryCollect(board, new GridPosition(2, 3), m_Cells));

            Assert.AreEqual(Height, m_Cells.Count);
            for (int y = 0; y < Height; y++)
            {
                AssertContains(new GridPosition(2, y));
            }
        }

        [Test]
        public void BombClearsSurroundingBlock()
        {
            Board board = CreateFilledBoard();
            PlaceSpecial(board, new GridPosition(2, 2), SpecialTileType.Bomb);

            Assert.IsTrue(m_Effects.TryCollect(board, new GridPosition(2, 2), m_Cells));

            Assert.AreEqual(BombCellCount, m_Cells.Count);
            for (int y = 2 - BombRadius; y <= 2 + BombRadius; y++)
            {
                for (int x = 2 - BombRadius; x <= 2 + BombRadius; x++)
                {
                    AssertContains(new GridPosition(x, y));
                }
            }
        }

        [Test]
        public void BombAtCornerIsClippedToBoard()
        {
            Board board = CreateFilledBoard();
            PlaceSpecial(board, new GridPosition(0, 0), SpecialTileType.Bomb);

            Assert.IsTrue(m_Effects.TryCollect(board, new GridPosition(0, 0), m_Cells));

            Assert.AreEqual(ClippedBombCellCount, m_Cells.Count);
        }

        [Test]
        public void ColorBombClearsEveryTileOfItsColour()
        {
            Board board = CreateFilledBoard();
            board.Set(new GridPosition(0, 1), new Tile(TargetColor));
            board.Set(new GridPosition(4, 4), new Tile(TargetColor));
            PlaceSpecial(board, new GridPosition(2, 2), SpecialTileType.ColorBomb, TargetColor);

            Assert.IsTrue(m_Effects.TryCollect(board, new GridPosition(2, 2), m_Cells));

            Assert.AreEqual(TargetColorCellCount, m_Cells.Count);
            AssertContains(new GridPosition(0, 1));
            AssertContains(new GridPosition(4, 4));
            AssertContains(new GridPosition(2, 2));
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

        private static void PlaceSpecial(
            Board board,
            GridPosition position,
            SpecialTileType type,
            TileColor color = FillerColor)
        {
            board.Set(position, new Tile(color, type));
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
