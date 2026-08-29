using System;
using System.Collections.Generic;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Gravity;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class GravityResolverTests
    {
        private const int Width = 3;
        private const int Height = 5;
        private const int Seed = 1;

        private GravityResolver m_Resolver;
        private List<TileMove> m_Moves;
        private List<TileSpawn> m_Spawns;

        [SetUp]
        public void SetUp()
        {
            m_Resolver = new GravityResolver(new TestBoardSettings(Width, Height), new Random(Seed));
            m_Moves = new List<TileMove>();
            m_Spawns = new List<TileSpawn>();
        }

        [Test]
        public void SingleHoleDropsTilesAboveByOne()
        {
            Board board = CreateFullBoard();
            board.Set(new GridPosition(1, 2), Tile.Empty);

            m_Resolver.Resolve(board, m_Moves, m_Spawns);

            Assert.AreEqual(2, m_Moves.Count);
            AssertContainsMove(new GridPosition(1, 3), new GridPosition(1, 2));
            AssertContainsMove(new GridPosition(1, 4), new GridPosition(1, 3));
            Assert.AreEqual(1, m_Spawns.Count);
            Assert.AreEqual(new GridPosition(1, 4), m_Spawns[0].Position);
            AssertFull(board);
        }

        [Test]
        public void HoleClusterInColumnMiddleCollapsesToBottom()
        {
            Board board = CreateFullBoard();
            board.Set(new GridPosition(0, 1), Tile.Empty);
            board.Set(new GridPosition(0, 2), Tile.Empty);
            board.Set(new GridPosition(0, 3), Tile.Empty);

            m_Resolver.Resolve(board, m_Moves, m_Spawns);

            Assert.AreEqual(1, m_Moves.Count);
            AssertContainsMove(new GridPosition(0, 4), new GridPosition(0, 1));
            Assert.AreEqual(3, m_Spawns.Count);
            AssertFull(board);
        }

        [Test]
        public void EmptyColumnIsFullyRespawnedWithoutMoves()
        {
            Board board = CreateFullBoard();
            for (int y = 0; y < Height; y++)
            {
                board.Set(new GridPosition(2, y), Tile.Empty);
            }

            m_Resolver.Resolve(board, m_Moves, m_Spawns);

            Assert.AreEqual(0, m_Moves.Count);
            Assert.AreEqual(Height, m_Spawns.Count);
            AssertFull(board);
        }

        [Test]
        public void UntouchedBoardProducesNothing()
        {
            Board board = CreateFullBoard();

            m_Resolver.Resolve(board, m_Moves, m_Spawns);

            Assert.AreEqual(0, m_Moves.Count);
            Assert.AreEqual(0, m_Spawns.Count);
        }

        private void AssertContainsMove(GridPosition from, GridPosition to)
        {
            for (int index = 0; index < m_Moves.Count; index++)
            {
                if (m_Moves[index].From.Equals(from) && m_Moves[index].To.Equals(to))
                {
                    return;
                }
            }

            Assert.Fail($"Missing move {from.X},{from.Y} -> {to.X},{to.Y}");
        }

        private static void AssertFull(Board board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board.TryGet(new GridPosition(x, y), out Tile tile);
                    Assert.IsFalse(tile.IsEmpty);
                }
            }
        }

        private static Board CreateFullBoard()
        {
            Board board = new Board(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    board.Set(new GridPosition(x, y), new Tile(TileColor.Red));
                }
            }

            return board;
        }
    }
}
