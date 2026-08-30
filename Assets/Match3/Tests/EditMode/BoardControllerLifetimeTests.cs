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
    public sealed class BoardControllerLifetimeTests
    {
        private const int Width = 4;
        private const int Height = 4;

        private static readonly TileColor[,] PlayableLayout =
        {
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Blue },
            { TileColor.Green, TileColor.Red, TileColor.Blue, TileColor.Green },
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Blue },
            { TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red }
        };

        private GamePipe m_GamePipe;
        private ProjectPipe m_ProjectPipe;
        private BoardController m_Controller;
        private int m_SignalCount;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_SignalCount = 0;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<SwapAcceptedSignal>(OnSwapAccepted);
            m_GamePipe.SubscribeTo<SwapRejectedSignal>(OnSwapRejected);
            m_GamePipe.SubscribeTo<MatchesResolvedSignal>(OnMatchesResolved);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);

            TestBoardSettings settings = new TestBoardSettings(Width, Height);
            MatchFinder matchFinder = new MatchFinder(settings);
            m_Controller = new BoardController(
                m_GamePipe,
                m_ProjectPipe,
                settings,
                new StubBoardGenerator(PlayableLayout),
                matchFinder,
                new StubGravityResolver(PlayableLayout),
                new MoveScanner(matchFinder),
                new ChainResolver(new SpecialTileEffects()),
                new SpecialCombinationResolver(),
                new FakeSaveRepository());
        }

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
            m_ProjectPipe.Dispose();
        }

        [Test]
        public void DisposeStopsCascadeWithoutFurtherSignals()
        {
            m_ProjectPipe.Raise(new RoundStartedSignal(false));
            m_GamePipe.Raise(new SwapRequestedSignal(new GridPosition(1, 1), new GridPosition(1, 2)));

            m_Controller.Dispose();
            int countAfterDispose = m_SignalCount;
            m_GamePipe.Raise(new BoardAnimationCompletedSignal());
            m_GamePipe.Raise(new BoardAnimationCompletedSignal());

            Assert.AreEqual(countAfterDispose, m_SignalCount);
        }

        [Test]
        public void DisposedControllerIgnoresAllSignals()
        {
            m_Controller.Dispose();
            m_SignalCount = 0;

            m_ProjectPipe.Raise(new RoundStartedSignal(false));
            m_GamePipe.Raise(new SwapRequestedSignal(new GridPosition(1, 1), new GridPosition(1, 2)));

            Assert.AreEqual(0, m_SignalCount);
        }

        [Test]
        public void DisposeIsIdempotent()
        {
            m_Controller.Dispose();

            Assert.DoesNotThrow(() => m_Controller.Dispose());
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal) => m_SignalCount++;

        private void OnSwapAccepted(ref SwapAcceptedSignal signal) => m_SignalCount++;

        private void OnSwapRejected(ref SwapRejectedSignal signal) => m_SignalCount++;

        private void OnMatchesResolved(ref MatchesResolvedSignal signal) => m_SignalCount++;

        private void OnInputLockChanged(ref InputLockChangedSignal signal) => m_SignalCount++;
    }
}
