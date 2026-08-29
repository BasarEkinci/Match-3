using System.Collections.Generic;
using Match3.Controller;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Matching;
using Match3.Model.Special;
using Match3.Signals;
using NUnit.Framework;
using Syntac.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class SpecialCombinationSwapTests
    {
        private const int Width = 5;
        private const int Height = 5;
        private const int AnimationStepLimit = 100;

        private static readonly GridPosition First = new GridPosition(1, 1);
        private static readonly GridPosition Second = new GridPosition(2, 1);
        private static readonly GridPosition PlainFrom = new GridPosition(0, 0);
        private static readonly GridPosition PlainTo = new GridPosition(1, 0);

        private static readonly TileColor[,] Layout =
        {
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Green, TileColor.Blue },
            { TileColor.Green, TileColor.Blue, TileColor.Green, TileColor.Red, TileColor.Green },
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Green, TileColor.Blue },
            { TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Green },
            { TileColor.Green, TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red }
        };

        private GamePipe m_GamePipe;
        private ProjectPipe m_ProjectPipe;
        private BoardController m_Controller;
        private Board m_Board;
        private List<GridPosition> m_Cleared;
        private int m_AcceptedCount;
        private int m_RejectedCount;
        private int m_CombinationCount;
        private bool m_HasUnlocked;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_Cleared = new List<GridPosition>();
            m_AcceptedCount = 0;
            m_RejectedCount = 0;
            m_CombinationCount = 0;
            m_HasUnlocked = false;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<SwapAcceptedSignal>(OnSwapAccepted);
            m_GamePipe.SubscribeTo<SwapRejectedSignal>(OnSwapRejected);
            m_GamePipe.SubscribeTo<SpecialCombinationTriggeredSignal>(OnCombinationTriggered);
            m_GamePipe.SubscribeTo<CellsClearedSignal>(OnCellsCleared);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);

            TestBoardSettings settings = new TestBoardSettings(Width, Height);
            MatchFinder matchFinder = new MatchFinder(settings);
            m_Controller = new BoardController(
                m_GamePipe,
                m_ProjectPipe,
                settings,
                new StubBoardGenerator(Layout),
                matchFinder,
                new StubGravityResolver(Layout),
                new MoveScanner(matchFinder),
                new ChainResolver(new SpecialTileEffects()),
                new SpecialCombinationResolver(),
                TestBoosters.Empty(),
                new FakeSaveRepository());

            m_ProjectPipe.Raise(new RoundStartedSignal(false));
        }

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
            m_ProjectPipe.Dispose();
        }

        [Test]
        public void SwappingTwoSpecialsIsAlwaysAccepted()
        {
            PlaceSpecials(SpecialTileType.HorizontalRocket, SpecialTileType.VerticalRocket);

            Swap();

            Assert.AreEqual(1, m_AcceptedCount);
            Assert.AreEqual(0, m_RejectedCount);
            Assert.AreEqual(1, m_CombinationCount);
        }

        [Test]
        public void RocketCombinationClearsRowAndColumn()
        {
            PlaceSpecials(SpecialTileType.HorizontalRocket, SpecialTileType.VerticalRocket);

            Swap();

            for (int x = 0; x < Width; x++)
            {
                AssertCleared(new GridPosition(x, Second.Y));
            }

            for (int y = 0; y < Height; y++)
            {
                AssertCleared(new GridPosition(Second.X, y));
            }
        }

        [Test]
        public void SwappingOneSpecialWithAPlainTileStaysANormalMove()
        {
            m_Board.Set(PlainFrom, new Tile(TileColor.Purple, SpecialTileType.Bomb));

            m_GamePipe.Raise(new SwapRequestedSignal(PlainFrom, PlainTo));

            Assert.AreEqual(0, m_AcceptedCount);
            Assert.AreEqual(1, m_RejectedCount);
            Assert.AreEqual(0, m_CombinationCount);
        }

        [Test]
        public void ColourBombSwappedWithAPlainTileIsAcceptedAndSweepsThatColour()
        {
            m_Board.Set(First, new Tile(TileColor.Purple, SpecialTileType.ColorBomb));
            m_Board.Set(Second, new Tile(TileColor.Yellow));
            m_Board.Set(new GridPosition(4, 4), new Tile(TileColor.Yellow));
            m_Board.Set(new GridPosition(0, 3), new Tile(TileColor.Yellow));

            Swap();

            Assert.AreEqual(1, m_AcceptedCount);
            Assert.AreEqual(0, m_RejectedCount);
            Assert.AreEqual(1, m_CombinationCount);
            AssertCleared(new GridPosition(4, 4));
            AssertCleared(new GridPosition(0, 3));
            AssertCleared(First);
            AssertCleared(Second);
        }

        private void PlaceSpecials(SpecialTileType first, SpecialTileType second)
        {
            m_Board.Set(First, new Tile(TileColor.Purple, first));
            m_Board.Set(Second, new Tile(TileColor.Purple, second));
        }

        private void Swap()
        {
            m_GamePipe.Raise(new SwapRequestedSignal(First, Second));
            for (int step = 0; step < AnimationStepLimit && !m_HasUnlocked; step++)
            {
                m_GamePipe.Raise(new BoardAnimationCompletedSignal());
            }
        }

        private void AssertCleared(GridPosition position)
        {
            for (int index = 0; index < m_Cleared.Count; index++)
            {
                if (m_Cleared[index].Equals(position))
                {
                    return;
                }
            }

            Assert.Fail($"Cell {position.X},{position.Y} was not cleared");
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal) => m_Board = signal.Board;

        private void OnSwapAccepted(ref SwapAcceptedSignal signal) => m_AcceptedCount++;

        private void OnSwapRejected(ref SwapRejectedSignal signal) => m_RejectedCount++;

        private void OnCombinationTriggered(ref SpecialCombinationTriggeredSignal signal) => m_CombinationCount++;

        private void OnInputLockChanged(ref InputLockChangedSignal signal) => m_HasUnlocked = !signal.IsLocked;

        private void OnCellsCleared(ref CellsClearedSignal signal)
        {
            for (int index = 0; index < signal.Cells.Count; index++)
            {
                m_Cleared.Add(signal.Cells[index]);
            }
        }
    }
}
