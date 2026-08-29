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
    public sealed class SpecialTileCreationTests
    {
        private const int Width = 5;
        private const int Height = 5;
        private const int AnimationStepLimit = 100;

        private static readonly TileColor[,] QuadLayout =
        {
            { TileColor.Red, TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Blue },
            { TileColor.Green, TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Blue },
            { TileColor.Blue, TileColor.Red, TileColor.Blue, TileColor.Red, TileColor.Green },
            { TileColor.Green, TileColor.Green, TileColor.Red, TileColor.Blue, TileColor.Red },
            { TileColor.Red, TileColor.Blue, TileColor.Green, TileColor.Green, TileColor.Blue }
        };

        private static readonly TileColor[,] CornerLayout =
        {
            { TileColor.Blue, TileColor.Green, TileColor.Red, TileColor.Green, TileColor.Blue },
            { TileColor.Green, TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red },
            { TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Red, TileColor.Red },
            { TileColor.Green, TileColor.Blue, TileColor.Red, TileColor.Green, TileColor.Blue },
            { TileColor.Blue, TileColor.Green, TileColor.Blue, TileColor.Red, TileColor.Green }
        };

        private GamePipe m_GamePipe;
        private ProjectPipe m_ProjectPipe;
        private BoardController m_Controller;
        private Board m_Board;
        private List<SpecialTileCreatedSignal> m_Created;
        private List<SpecialTileType> m_BoardSpecials;
        private bool m_HasUnlocked;

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
            m_ProjectPipe.Dispose();
        }

        [Test]
        public void HorizontalQuadCreatesRocketOnTheSwappedCell()
        {
            Start(QuadLayout);

            Swap(new GridPosition(2, 0), new GridPosition(2, 1));

            Assert.AreEqual(1, m_Created.Count);
            Assert.AreEqual(SpecialTileType.HorizontalRocket, m_Created[0].Type);
            Assert.AreEqual(new GridPosition(2, 0), m_Created[0].Position);
            Assert.AreEqual(SpecialTileType.HorizontalRocket, m_BoardSpecials[0]);
        }

        [Test]
        public void IntersectingRunsCreateExactlyOneBomb()
        {
            Start(CornerLayout);

            Swap(new GridPosition(1, 2), new GridPosition(2, 2));

            Assert.AreEqual(1, m_Created.Count);
            Assert.AreEqual(SpecialTileType.Bomb, m_Created[0].Type);
            Assert.AreEqual(new GridPosition(2, 2), m_Created[0].Position);
            Assert.AreEqual(SpecialTileType.Bomb, m_BoardSpecials[0]);
        }

        private void Start(TileColor[,] layout)
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_Created = new List<SpecialTileCreatedSignal>();
            m_BoardSpecials = new List<SpecialTileType>();
            m_HasUnlocked = false;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<SpecialTileCreatedSignal>(OnSpecialTileCreated);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);

            TestBoardSettings settings = new TestBoardSettings(Width, Height);
            MatchFinder matchFinder = new MatchFinder(settings);
            m_Controller = new BoardController(
                m_GamePipe,
                m_ProjectPipe,
                settings,
                new StubBoardGenerator(layout),
                matchFinder,
                new StubGravityResolver(layout),
                new MoveScanner(matchFinder),
                new ChainResolver(new SpecialTileEffects()),
                new SpecialCombinationResolver());

            m_ProjectPipe.Raise(new RoundStartedSignal());
        }

        private void Swap(GridPosition from, GridPosition to)
        {
            m_GamePipe.Raise(new SwapRequestedSignal(from, to));
            for (int step = 0; step < AnimationStepLimit && !m_HasUnlocked; step++)
            {
                m_GamePipe.Raise(new BoardAnimationCompletedSignal());
            }
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal) => m_Board = signal.Board;

        private void OnInputLockChanged(ref InputLockChangedSignal signal) => m_HasUnlocked = !signal.IsLocked;

        private void OnSpecialTileCreated(ref SpecialTileCreatedSignal signal)
        {
            m_Created.Add(signal);
            m_Board.TryGet(signal.Position, out Tile tile);
            m_BoardSpecials.Add(tile.Special);
        }
    }
}
