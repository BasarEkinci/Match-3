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
    public sealed class BoardShuffleTests
    {
        private const int Width = 4;
        private const int Height = 4;
        private const int AnimationStepLimit = 100;

        private static readonly TileColor[,] PlayableLayout =
        {
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Blue },
            { TileColor.Green, TileColor.Red, TileColor.Blue, TileColor.Green },
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Blue },
            { TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red }
        };

        private static readonly TileColor[,] DeadlockLayout =
        {
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Green },
            { TileColor.Blue, TileColor.Green, TileColor.Blue, TileColor.Red },
            { TileColor.Red, TileColor.Red, TileColor.Blue, TileColor.Blue },
            { TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red }
        };

        private GamePipe m_GamePipe;
        private ProjectPipe m_ProjectPipe;
        private BoardController m_Controller;
        private MatchFinder m_MatchFinder;
        private MoveScanner m_MoveScanner;
        private Board m_Board;
        private int m_ShuffleStartedCount;
        private int m_ShuffleCompletedCount;
        private bool m_HasUnlocked;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_ShuffleStartedCount = 0;
            m_ShuffleCompletedCount = 0;
            m_HasUnlocked = false;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<BoardShuffleStartedSignal>(OnShuffleStarted);
            m_GamePipe.SubscribeTo<BoardShuffleCompletedSignal>(OnShuffleCompleted);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);

            TestBoardSettings settings = new TestBoardSettings(Width, Height);
            m_MatchFinder = new MatchFinder(settings);
            m_MoveScanner = new MoveScanner(m_MatchFinder);
            m_Controller = new BoardController(
                m_GamePipe,
                m_ProjectPipe,
                settings,
                new StubBoardGenerator(PlayableLayout),
                m_MatchFinder,
                new StubGravityResolver(DeadlockLayout),
                m_MoveScanner,
                new ChainResolver(new SpecialTileEffects()),
                new SpecialCombinationResolver());
        }

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
            m_ProjectPipe.Dispose();
        }

        [Test]
        public void DeadlockedBoardIsReshuffledIntoPlayableState()
        {
            m_ProjectPipe.Raise(new RoundStartedSignal());
            m_GamePipe.Raise(new SwapRequestedSignal(new GridPosition(1, 1), new GridPosition(1, 2)));
            CompleteAnimations();

            Assert.AreEqual(1, m_ShuffleStartedCount);
            Assert.AreEqual(1, m_ShuffleCompletedCount);
            Assert.AreEqual(0, m_MatchFinder.FindMatches(m_Board).Count);
            Assert.IsTrue(m_MoveScanner.HasAnyMove(m_Board));
            Assert.IsTrue(m_HasUnlocked);
        }

        [Test]
        public void PlayableBoardIsNotReshuffled()
        {
            m_Controller.Dispose();
            m_Controller = new BoardController(
                m_GamePipe,
                m_ProjectPipe,
                new TestBoardSettings(Width, Height),
                new StubBoardGenerator(PlayableLayout),
                m_MatchFinder,
                new StubGravityResolver(PlayableLayout),
                m_MoveScanner,
                new ChainResolver(new SpecialTileEffects()),
                new SpecialCombinationResolver());

            m_ProjectPipe.Raise(new RoundStartedSignal());
            m_GamePipe.Raise(new SwapRequestedSignal(new GridPosition(1, 1), new GridPosition(1, 2)));
            CompleteAnimations();

            Assert.AreEqual(0, m_ShuffleStartedCount);
            Assert.IsTrue(m_HasUnlocked);
        }

        private void CompleteAnimations()
        {
            for (int step = 0; step < AnimationStepLimit && !m_HasUnlocked; step++)
            {
                m_GamePipe.Raise(new BoardAnimationCompletedSignal());
            }
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal) => m_Board = signal.Board;

        private void OnShuffleStarted(ref BoardShuffleStartedSignal signal) => m_ShuffleStartedCount++;

        private void OnShuffleCompleted(ref BoardShuffleCompletedSignal signal) => m_ShuffleCompletedCount++;

        private void OnInputLockChanged(ref InputLockChangedSignal signal) => m_HasUnlocked = !signal.IsLocked;
    }
}
