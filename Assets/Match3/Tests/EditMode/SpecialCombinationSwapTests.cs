using System.Collections.Generic;
using Match3.Controller;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Matching;
using Match3.Model.Special;
using Match3.Signals;
using NUnit.Framework;
using Match3.Core.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class SpecialCombinationSwapTests
    {
        private const int Width = 5;
        private const int Height = 5;
        private const int AnimationStepLimit = 100;

        private static readonly GridPosition First = new GridPosition(1, 1);
        private static readonly GridPosition Second = new GridPosition(2, 1);
        private static readonly GridPosition Above = new GridPosition(1, 2);
        private static readonly GridPosition PlainFrom = new GridPosition(0, 0);
        private static readonly GridPosition PlainTo = new GridPosition(1, 0);
        private static readonly GridPosition BombColorCell = new GridPosition(0, 2);
        private static readonly GridPosition PartnerColorCell = new GridPosition(4, 0);
        private static readonly GridPosition ConvertedCell = new GridPosition(0, 3);

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
        private List<ClearedCell> m_Cleared;
        private int m_AcceptedCount;
        private int m_RejectedCount;
        private int m_CombinationCount;
        private int m_ConversionCount;
        private SpecialConversionSignal m_LastConversion;
        private bool m_HasUnlocked;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_Cleared = new List<ClearedCell>();
            m_AcceptedCount = 0;
            m_RejectedCount = 0;
            m_CombinationCount = 0;
            m_ConversionCount = 0;
            m_HasUnlocked = false;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<SwapAcceptedSignal>(OnSwapAccepted);
            m_GamePipe.SubscribeTo<SwapRejectedSignal>(OnSwapRejected);
            m_GamePipe.SubscribeTo<SpecialCombinationTriggeredSignal>(OnCombinationTriggered);
            m_GamePipe.SubscribeTo<SpecialConversionSignal>(OnConversion);
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
        }

        [Test]
        public void EachRocketClearsItsOwnAxisAfterTheSwap()
        {
            PlaceSpecials(SpecialTileType.HorizontalRocket, SpecialTileType.VerticalRocket);

            Swap();

            for (int x = 0; x < Width; x++)
            {
                AssertCleared(new GridPosition(x, Second.Y));
            }

            for (int y = 0; y < Height; y++)
            {
                AssertCleared(new GridPosition(First.X, y));
            }
        }

        [Test]
        public void TwoStackedHorizontalRocketsClearBothRows()
        {
            m_Board.Set(First, new Tile(TileColor.Purple, SpecialTileType.HorizontalRocket));
            m_Board.Set(Above, new Tile(TileColor.Purple, SpecialTileType.HorizontalRocket));

            Swap(First, Above);

            for (int x = 0; x < Width; x++)
            {
                AssertCleared(new GridPosition(x, First.Y));
                AssertCleared(new GridPosition(x, Above.Y));
            }
        }

        [Test]
        public void SwappingABombWithAPlainTileDetonatesIt()
        {
            m_Board.Set(First, new Tile(TileColor.Purple, SpecialTileType.Bomb));

            Swap();

            Assert.AreEqual(1, m_AcceptedCount);
            Assert.AreEqual(0, m_RejectedCount);
            for (int y = Second.Y - 1; y <= Second.Y + 1; y++)
            {
                for (int x = Second.X - 1; x <= Second.X + 1; x++)
                {
                    AssertCleared(new GridPosition(x, y));
                }
            }
        }

        [Test]
        public void ABombSwappedIntoAMatchCreatingMoveStillDetonates()
        {
            m_Board.Set(First, new Tile(TileColor.Purple, SpecialTileType.Bomb));
            m_Board.Set(Second, new Tile(TileColor.Yellow));
            m_Board.Set(new GridPosition(First.X, First.Y - 1), new Tile(TileColor.Yellow));
            m_Board.Set(Above, new Tile(TileColor.Yellow));

            Swap();

            AssertCleared(First);
            AssertCleared(Above);
            for (int y = Second.Y - 1; y <= Second.Y + 1; y++)
            {
                AssertCleared(new GridPosition(Second.X + 1, y));
            }
        }

        [Test]
        public void TappingARocketClearsItsRow()
        {
            m_Board.Set(First, new Tile(TileColor.Purple, SpecialTileType.HorizontalRocket));

            Activate(First);

            for (int x = 0; x < Width; x++)
            {
                AssertCleared(new GridPosition(x, First.Y));
            }
        }

        [Test]
        public void TappingAPlainTileDoesNothing()
        {
            Activate(PlainFrom);

            Assert.AreEqual(0, m_Cleared.Count);
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

        [Test]
        public void ColourBombSweepsOnlyThePartnerColourNotItsOwn()
        {
            m_Board.Set(First, new Tile(TileColor.Red, SpecialTileType.ColorBomb));
            m_Board.Set(Second, new Tile(TileColor.Blue));
            m_Board.Set(PartnerColorCell, new Tile(TileColor.Blue));
            m_Board.Set(BombColorCell, new Tile(TileColor.Red));

            Swap();

            AssertCleared(PartnerColorCell);
            AssertNotCleared(BombColorCell);
        }

        [Test]
        public void ColourBombAndBombConvertsThePartnerColourAndDetonatesIt()
        {
            m_Board.Set(First, new Tile(TileColor.Purple, SpecialTileType.ColorBomb));
            m_Board.Set(Second, new Tile(TileColor.Yellow, SpecialTileType.Bomb));
            m_Board.Set(ConvertedCell, new Tile(TileColor.Yellow));

            Swap();

            Assert.AreEqual(1, m_ConversionCount);
            Assert.AreEqual(TileColor.Yellow, m_LastConversion.Color);
            Assert.AreEqual(SpecialTileType.Bomb, m_LastConversion.Special);
            AssertCleared(ConvertedCell);
            AssertCleared(new GridPosition(ConvertedCell.X + 1, ConvertedCell.Y + 1));
            AssertCleared(new GridPosition(ConvertedCell.X + 1, ConvertedCell.Y - 1));
        }

        private void PlaceSpecials(SpecialTileType first, SpecialTileType second)
        {
            m_Board.Set(First, new Tile(TileColor.Purple, first));
            m_Board.Set(Second, new Tile(TileColor.Purple, second));
        }

        private void Swap() => Swap(First, Second);

        private void Swap(GridPosition from, GridPosition to)
        {
            m_GamePipe.Raise(new SwapRequestedSignal(from, to));
            PumpAnimations();
        }

        private void Activate(GridPosition position)
        {
            m_GamePipe.Raise(new SpecialActivationRequestedSignal(position));
            PumpAnimations();
        }

        private void PumpAnimations()
        {
            for (int step = 0; step < AnimationStepLimit && !m_HasUnlocked; step++)
            {
                m_GamePipe.Raise(new BoardAnimationCompletedSignal());
            }
        }

        private void AssertCleared(GridPosition position)
        {
            for (int index = 0; index < m_Cleared.Count; index++)
            {
                if (m_Cleared[index].Position.Equals(position))
                {
                    return;
                }
            }

            Assert.Fail($"Cell {position.X},{position.Y} was not cleared");
        }

        private void AssertNotCleared(GridPosition position)
        {
            for (int index = 0; index < m_Cleared.Count; index++)
            {
                Assert.IsFalse(
                    m_Cleared[index].Position.Equals(position),
                    $"Cell {position.X},{position.Y} should not have been cleared");
            }
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal) => m_Board = signal.Board;

        private void OnSwapAccepted(ref SwapAcceptedSignal signal) => m_AcceptedCount++;

        private void OnSwapRejected(ref SwapRejectedSignal signal) => m_RejectedCount++;

        private void OnCombinationTriggered(ref SpecialCombinationTriggeredSignal signal) => m_CombinationCount++;

        private void OnConversion(ref SpecialConversionSignal signal)
        {
            m_ConversionCount++;
            m_LastConversion = signal;
        }

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
