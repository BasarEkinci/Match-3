using System;
using Match3.Model;
using Match3.Model.Generation;
using Match3.Model.Matching;
using Match3.Model.Settings;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class BoardGeneratorTests
    {
        private const int Width = 8;
        private const int Height = 8;
        private const int SeedCount = 200;

        [Test]
        public void GeneratedBoardsHaveNoMatchAndAtLeastOneMove([Range(0, SeedCount - 1)] int seed)
        {
            IBoardSettings settings = new TestBoardSettings(Width, Height);
            MatchFinder matchFinder = new MatchFinder(settings);
            MoveScanner moveScanner = new MoveScanner(matchFinder);
            BoardGenerator generator =
                new BoardGenerator(settings, matchFinder, moveScanner, new Random(seed));

            Board board = new Board(Width, Height);
            generator.Generate(board);

            Assert.AreEqual(0, matchFinder.FindMatches(board).Count);
            Assert.IsTrue(moveScanner.HasAnyMove(board));
            AssertFull(board);
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
    }
}
