using Match3.Controller;
using Match3.Model;
using Match3.Model.Boosters;
using Match3.Model.Enums;
using Match3.Model.Generation;
using Match3.Model.Matching;
using Match3.Model.Special;
using Match3.Signals;
using NUnit.Framework;
using Syntac.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class ShuffleBoosterTests
    {
        private const int Width = 5;
        private const int Height = 5;
        private const int AnimationStepLimit = 100;

        private static readonly GridPosition Sample = new GridPosition(0, 0);

        private static readonly TileColor[,] FirstLayout =
        {
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Green, TileColor.Blue },
            { TileColor.Green, TileColor.Blue, TileColor.Green, TileColor.Red, TileColor.Green },
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Green, TileColor.Blue },
            { TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Green },
            { TileColor.Green, TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red }
        };

        private static readonly TileColor[,] SecondLayout =
        {
            { TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Green },
            { TileColor.Red, TileColor.Green, TileColor.Blue, TileColor.Green, TileColor.Blue },
            { TileColor.Green, TileColor.Blue, TileColor.Red, TileColor.Blue, TileColor.Red },
            { TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Green },
            { TileColor.Red, TileColor.Green, TileColor.Blue, TileColor.Green, TileColor.Blue }
        };

        private GamePipe m_GamePipe;
        private ProjectPipe m_ProjectPipe;
        private BoosterModel m_Boosters;
        private BoardController m_Controller;
        private Board m_Board;
        private int m_ShuffleStartedCount;
        private int m_ShuffleCompletedCount;
        private int m_ClearedCount;
        private int m_AppliedCount;
        private bool m_IsInputLocked;
        private bool m_HasUnlocked;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_ShuffleStartedCount = 0;
            m_ShuffleCompletedCount = 0;
            m_ClearedCount = 0;
            m_AppliedCount = 0;
            m_IsInputLocked = false;
            m_HasUnlocked = false;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<BoardShuffleStartedSignal>(OnShuffleStarted);
            m_GamePipe.SubscribeTo<BoardShuffleCompletedSignal>(OnShuffleCompleted);
            m_GamePipe.SubscribeTo<CellsClearedSignal>(OnCellsCleared);
            m_GamePipe.SubscribeTo<BoosterAppliedSignal>(OnBoosterApplied);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);

            TestBoardSettings settings = new TestBoardSettings(Width, Height);
            MatchFinder matchFinder = new MatchFinder(settings);
            m_Boosters = TestBoosters.Carrying(BoosterType.Shuffle);
            m_Controller = new BoardController(
                m_GamePipe,
                m_ProjectPipe,
                settings,
                new AlternatingBoardGenerator(FirstLayout, SecondLayout),
                matchFinder,
                new StubGravityResolver(FirstLayout),
                new MoveScanner(matchFinder),
                new ChainResolver(new SpecialTileEffects()),
                new SpecialCombinationResolver(),
                m_Boosters,
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
        public void ShuffleRedistributesBoardWithoutClearingTiles()
        {
            Assert.AreEqual(TileColor.Red, ColorAt(Sample));

            UseShuffle();
            CompleteAnimations();

            Assert.AreEqual(1, m_AppliedCount);
            Assert.AreEqual(1, m_ShuffleStartedCount);
            Assert.AreEqual(1, m_ShuffleCompletedCount);
            Assert.AreEqual(0, m_ClearedCount);
            Assert.AreEqual(TileColor.Blue, ColorAt(Sample));
            Assert.AreEqual(0, m_Boosters.CountOf(BoosterType.Shuffle));
        }

        [Test]
        public void ShuffleLocksInputUntilBoardIsPlayableAgain()
        {
            UseShuffle();

            Assert.IsTrue(m_IsInputLocked);

            CompleteAnimations();

            Assert.IsTrue(m_HasUnlocked);
            Assert.IsFalse(m_IsInputLocked);
        }

        [Test]
        public void ShuffleWithoutInventoryDoesNothing()
        {
            UseShuffle();
            CompleteAnimations();

            UseShuffle();

            Assert.AreEqual(1, m_AppliedCount);
            Assert.AreEqual(1, m_ShuffleStartedCount);
        }

        private void UseShuffle()
        {
            m_GamePipe.Raise(new BoosterUseRequestedSignal(BoosterType.Shuffle, Sample));
        }

        private void CompleteAnimations()
        {
            for (int step = 0; step < AnimationStepLimit && !m_HasUnlocked; step++)
            {
                m_GamePipe.Raise(new BoardAnimationCompletedSignal());
            }
        }

        private TileColor ColorAt(GridPosition position)
        {
            m_Board.TryGet(position, out Tile tile);
            return tile.Color;
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal) => m_Board = signal.Board;

        private void OnShuffleStarted(ref BoardShuffleStartedSignal signal) => m_ShuffleStartedCount++;

        private void OnShuffleCompleted(ref BoardShuffleCompletedSignal signal) => m_ShuffleCompletedCount++;

        private void OnCellsCleared(ref CellsClearedSignal signal) => m_ClearedCount += signal.Cells.Count;

        private void OnBoosterApplied(ref BoosterAppliedSignal signal) => m_AppliedCount++;

        private void OnInputLockChanged(ref InputLockChangedSignal signal)
        {
            m_IsInputLocked = signal.IsLocked;
            m_HasUnlocked = !signal.IsLocked;
        }

        private sealed class AlternatingBoardGenerator : IBoardGenerator
        {
            private readonly TileColor[,] m_First;
            private readonly TileColor[,] m_Second;

            private bool m_HasGenerated;

            public AlternatingBoardGenerator(TileColor[,] first, TileColor[,] second)
            {
                m_First = first;
                m_Second = second;
            }

            public void Generate(Board board)
            {
                TileColor[,] layout = m_HasGenerated ? m_Second : m_First;
                m_HasGenerated = true;
                for (int y = 0; y < board.Height; y++)
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        board.Set(new GridPosition(x, y), new Tile(layout[y, x]));
                    }
                }
            }
        }
    }
}
