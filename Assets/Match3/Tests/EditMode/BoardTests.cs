using System;
using Match3.Model;
using Match3.Model.Enums;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class BoardTests
    {
        private const int Width = 4;
        private const int Height = 3;

        [Test]
        public void NewBoardIsEmpty()
        {
            Board board = new Board(Width, Height);

            Assert.IsTrue(board.TryGet(new GridPosition(0, 0), out Tile tile));
            Assert.IsTrue(tile.IsEmpty);
        }

        [Test]
        public void SetThenTryGetReturnsSameTile()
        {
            Board board = new Board(Width, Height);
            GridPosition position = new GridPosition(2, 1);

            board.Set(position, new Tile(TileColor.Blue, SpecialTileType.Bomb));

            Assert.IsTrue(board.TryGet(position, out Tile tile));
            Assert.AreEqual(TileColor.Blue, tile.Color);
            Assert.AreEqual(SpecialTileType.Bomb, tile.Special);
            Assert.IsFalse(tile.IsEmpty);
        }

        [Test]
        public void RowsDoNotAliasEachOther()
        {
            Board board = new Board(Width, Height);

            board.Set(new GridPosition(0, 1), new Tile(TileColor.Red));

            Assert.IsTrue(board.TryGet(new GridPosition(Width - 1, 0), out Tile previousRowTile));
            Assert.IsTrue(previousRowTile.IsEmpty);
        }

        [Test]
        public void SwapExchangesTiles()
        {
            Board board = new Board(Width, Height);
            GridPosition first = new GridPosition(0, 0);
            GridPosition second = new GridPosition(1, 0);
            board.Set(first, new Tile(TileColor.Red));
            board.Set(second, new Tile(TileColor.Green));

            board.Swap(first, second);

            board.TryGet(first, out Tile firstTile);
            board.TryGet(second, out Tile secondTile);
            Assert.AreEqual(TileColor.Green, firstTile.Color);
            Assert.AreEqual(TileColor.Red, secondTile.Color);
        }

        [Test]
        public void TryGetOutsideBoardFails()
        {
            Board board = new Board(Width, Height);

            Assert.IsFalse(board.TryGet(new GridPosition(-1, 0), out _));
            Assert.IsFalse(board.TryGet(new GridPosition(Width, 0), out _));
            Assert.IsFalse(board.TryGet(new GridPosition(0, Height), out _));
        }

        [Test]
        public void SetOutsideBoardThrows()
        {
            Board board = new Board(Width, Height);

            Assert.Throws<ArgumentOutOfRangeException>(() => board.Set(new GridPosition(-1, 1), new Tile(TileColor.Red)));
        }

        [Test]
        public void EqualPositionsShareHashCode()
        {
            Assert.AreEqual(new GridPosition(2, 3), new GridPosition(2, 3));
            Assert.AreEqual(new GridPosition(2, 3).GetHashCode(), new GridPosition(2, 3).GetHashCode());
            Assert.AreNotEqual(new GridPosition(2, 3), new GridPosition(3, 2));
        }
    }
}
