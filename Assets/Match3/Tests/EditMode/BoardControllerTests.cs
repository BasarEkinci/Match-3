using System;
using Match3.Controller;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Gravity;
using Match3.Model.Matching;
using Match3.Model.Special;
using Match3.Signals;
using NUnit.Framework;
using Match3.Core.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class BoardControllerTests
    {
        private const int Width = 4;
        private const int Height = 4;
        private const int Seed = 1;
        private const int AnimationStepLimit = 100;

        private static readonly TileColor[,] Layout =
        {
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Blue },
            { TileColor.Green, TileColor.Red, TileColor.Blue, TileColor.Green },
            { TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Blue },
            { TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red }
        };

        private GamePipe m_GamePipe;
        private ProjectPipe m_ProjectPipe;
        private BoardController m_Controller;
        private FakeSaveRepository m_Save;
        private Board m_Board;
        private int m_AcceptedCount;
        private int m_ResolvedCount;
        private int m_RefilledCount;
        private int m_LastCascadeStep;
        private bool m_IsInputLocked;
        private bool m_HasUnlocked;
        private int m_RejectedCount;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_Save = new FakeSaveRepository();
            m_AcceptedCount = 0;
            m_RejectedCount = 0;
            m_ResolvedCount = 0;
            m_RefilledCount = 0;
            m_LastCascadeStep = 0;
            m_IsInputLocked = false;
            m_HasUnlocked = false;
            m_Board = null;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<SwapAcceptedSignal>(OnSwapAccepted);
            m_GamePipe.SubscribeTo<SwapRejectedSignal>(OnSwapRejected);
            m_GamePipe.SubscribeTo<MatchesResolvedSignal>(OnMatchesResolved);
            m_GamePipe.SubscribeTo<BoardRefilledSignal>(OnBoardRefilled);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);

            m_Controller = new BoardController(
                m_GamePipe,
                m_ProjectPipe,
                new TestBoardSettings(Width, Height),
                new StubBoardGenerator(Layout),
                new MatchFinder(new TestBoardSettings(Width, Height)),
                new GravityResolver(new TestBoardSettings(Width, Height), new Random(Seed)),
                new MoveScanner(new MatchFinder(new TestBoardSettings(Width, Height))),
                new ChainResolver(new SpecialTileEffects()),
                new SpecialCombinationResolver(),
                m_Save);
        }

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
            m_ProjectPipe.Dispose();
        }

        [Test]
        public void RoundStartPublishesCreatedBoard()
        {
            StartRound();

            Assert.IsNotNull(m_Board);
            Assert.AreEqual(TileColor.Green, ColorAt(1, 0));
        }

        [Test]
        public void NonAdjacentSwapIsRejectedAndLeavesBoardUntouched()
        {
            StartRound();

            RequestSwap(new GridPosition(0, 0), new GridPosition(2, 0));

            Assert.AreEqual(0, m_AcceptedCount);
            Assert.AreEqual(1, m_RejectedCount);
            Assert.AreEqual(TileColor.Red, ColorAt(0, 0));
            Assert.AreEqual(TileColor.Red, ColorAt(2, 0));
        }

        [Test]
        public void AdjacentSwapWithoutMatchIsRejectedAndReverted()
        {
            StartRound();

            RequestSwap(new GridPosition(0, 0), new GridPosition(1, 0));

            Assert.AreEqual(0, m_AcceptedCount);
            Assert.AreEqual(1, m_RejectedCount);
            Assert.AreEqual(TileColor.Red, ColorAt(0, 0));
            Assert.AreEqual(TileColor.Green, ColorAt(1, 0));
        }

        [Test]
        public void MatchingSwapIsAcceptedAndApplied()
        {
            StartRound();

            RequestSwap(new GridPosition(1, 1), new GridPosition(1, 2));

            Assert.AreEqual(1, m_AcceptedCount);
            Assert.AreEqual(0, m_RejectedCount);
            Assert.AreEqual(TileColor.Green, ColorAt(1, 1));
            Assert.AreEqual(TileColor.Red, ColorAt(1, 2));
        }

        [Test]
        public void InputIsIgnoredWhileNotIdle()
        {
            StartRound();
            RequestSwap(new GridPosition(1, 1), new GridPosition(1, 2));

            RequestSwap(new GridPosition(0, 0), new GridPosition(1, 0));

            Assert.AreEqual(1, m_AcceptedCount);
            Assert.AreEqual(0, m_RejectedCount);
        }

        [Test]
        public void AcceptedSwapLocksInputUntilCascadeEnds()
        {
            StartRound();

            RequestSwap(new GridPosition(1, 1), new GridPosition(1, 2));

            Assert.IsTrue(m_IsInputLocked);

            CompleteAnimations();

            Assert.IsTrue(m_HasUnlocked);
            Assert.IsFalse(m_IsInputLocked);
        }

        [Test]
        public void CascadeResolvesMatchesRefillsAndLeavesBoardPlayable()
        {
            StartRound();

            RequestSwap(new GridPosition(1, 1), new GridPosition(1, 2));
            CompleteAnimations();

            Assert.GreaterOrEqual(m_ResolvedCount, 1);
            Assert.AreEqual(m_ResolvedCount, m_RefilledCount);
            Assert.AreEqual(m_ResolvedCount, m_LastCascadeStep);
            Assert.AreEqual(0, new MatchFinder(new TestBoardSettings(Width, Height)).FindMatches(m_Board).Count);
            AssertBoardIsFull();
        }

        [Test]
        public void InputIsAcceptedAgainAfterCascade()
        {
            StartRound();
            RequestSwap(new GridPosition(1, 1), new GridPosition(1, 2));
            CompleteAnimations();

            RequestSwap(new GridPosition(0, 0), new GridPosition(2, 0));

            Assert.AreEqual(1, m_RejectedCount);
        }

        [Test]
        public void RestartingARoundCancelsTheCascadeAndUnlocksInput()
        {
            StartRound();
            RequestSwap(new GridPosition(1, 1), new GridPosition(1, 2));
            int resolvedBeforeRestart = m_ResolvedCount;

            StartRound();
            for (int step = 0; step < AnimationStepLimit; step++)
            {
                m_GamePipe.Raise(new BoardAnimationCompletedSignal());
            }

            Assert.IsFalse(m_IsInputLocked);
            Assert.AreEqual(resolvedBeforeRestart, m_ResolvedCount);
        }

        [Test]
        public void InputIsAcceptedRightAfterARestart()
        {
            StartRound();
            RequestSwap(new GridPosition(1, 1), new GridPosition(1, 2));

            StartRound();
            RequestSwap(new GridPosition(0, 0), new GridPosition(2, 0));

            Assert.AreEqual(1, m_RejectedCount);
        }

        [Test]
        public void ResumedRoundLoadsTheSavedBoard()
        {
            m_Save.Save(FilledBoard(TileColor.Yellow), 0);

            m_ProjectPipe.Raise(new RoundStartedSignal(true));

            Assert.AreEqual(TileColor.Yellow, ColorAt(0, 0));
            Assert.AreEqual(TileColor.Yellow, ColorAt(Width - 1, Height - 1));
        }

        private static Board FilledBoard(TileColor color)
        {
            Board board = new Board(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    board.Set(new GridPosition(x, y), new Tile(color));
                }
            }

            return board;
        }

        private void AssertBoardIsFull()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    m_Board.TryGet(new GridPosition(x, y), out Tile tile);
                    Assert.IsFalse(tile.IsEmpty);
                }
            }
        }

        private void StartRound()
        {
            m_ProjectPipe.Raise(new RoundStartedSignal(false));
        }

        private void RequestSwap(GridPosition from, GridPosition to)
        {
            m_GamePipe.Raise(new SwapRequestedSignal(from, to));
        }

        private TileColor ColorAt(int x, int y)
        {
            m_Board.TryGet(new GridPosition(x, y), out Tile tile);
            return tile.Color;
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal) => m_Board = signal.Board;

        private void OnSwapAccepted(ref SwapAcceptedSignal signal) => m_AcceptedCount++;

        private void OnSwapRejected(ref SwapRejectedSignal signal) => m_RejectedCount++;

        private void OnMatchesResolved(ref MatchesResolvedSignal signal)
        {
            m_ResolvedCount++;
            m_LastCascadeStep = signal.CascadeStep;
        }

        private void OnBoardRefilled(ref BoardRefilledSignal signal) => m_RefilledCount++;

        private void OnInputLockChanged(ref InputLockChangedSignal signal)
        {
            m_IsInputLocked = signal.IsLocked;
            m_HasUnlocked = !signal.IsLocked;
        }

        private void CompleteAnimations()
        {
            for (int step = 0; step < AnimationStepLimit && !m_HasUnlocked; step++)
            {
                m_GamePipe.Raise(new BoardAnimationCompletedSignal());
            }
        }
    }
}
