using System.Collections.Generic;
using Match3.Controller;
using Match3.Model;
using Match3.Model.Boosters;
using Match3.Model.Enums;
using Match3.Model.Matching;
using Match3.Model.Special;
using Match3.Signals;
using NUnit.Framework;
using Syntac.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class ColorPickerBoosterTests
    {
        private const int Width = 5;
        private const int Height = 5;
        private const int AnimationStepLimit = 100;
        private const int BlueCount = 5;

        private static readonly GridPosition BlueCell = new GridPosition(4, 0);

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
        private BoosterModel m_Boosters;
        private BoardController m_Controller;
        private Board m_Board;
        private List<GridPosition> m_Cleared;
        private int m_AppliedCount;
        private bool m_HasUnlocked;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_Cleared = new List<GridPosition>();
            m_AppliedCount = 0;
            m_HasUnlocked = false;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<CellsClearedSignal>(OnCellsCleared);
            m_GamePipe.SubscribeTo<BoosterAppliedSignal>(OnBoosterApplied);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);

            TestBoardSettings settings = new TestBoardSettings(Width, Height);
            MatchFinder matchFinder = new MatchFinder(settings);
            m_Boosters = TestBoosters.Carrying(BoosterType.ColorPicker);
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
        public void ColorPickerClearsEveryTileOfTargetColor()
        {
            UseColorPicker(BlueCell);

            Assert.AreEqual(1, m_AppliedCount);
            Assert.AreEqual(BlueCount, m_Cleared.Count);
            AssertAllClearedWere(TileColor.Blue);
            Assert.AreEqual(0, m_Boosters.CountOf(BoosterType.ColorPicker));
        }

        [Test]
        public void ColorPickerRefillsBoardAndUnlocksInput()
        {
            UseColorPicker(BlueCell);
            CompleteAnimations();

            Assert.IsTrue(m_HasUnlocked);
            m_Board.TryGet(BlueCell, out Tile tile);
            Assert.IsFalse(tile.IsEmpty);
        }

        [Test]
        public void ColorPickerOutsideBoardKeepsInventory()
        {
            UseColorPicker(new GridPosition(Width, Height));

            Assert.AreEqual(0, m_AppliedCount);
            Assert.AreEqual(1, m_Boosters.CountOf(BoosterType.ColorPicker));
        }

        private void AssertAllClearedWere(TileColor color)
        {
            for (int index = 0; index < m_Cleared.Count; index++)
            {
                Assert.AreEqual(color, Layout[m_Cleared[index].Y, m_Cleared[index].X]);
            }
        }

        private void UseColorPicker(GridPosition target)
        {
            m_GamePipe.Raise(new BoosterUseRequestedSignal(BoosterType.ColorPicker, target));
        }

        private void CompleteAnimations()
        {
            for (int step = 0; step < AnimationStepLimit && !m_HasUnlocked; step++)
            {
                m_GamePipe.Raise(new BoardAnimationCompletedSignal());
            }
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal) => m_Board = signal.Board;

        private void OnCellsCleared(ref CellsClearedSignal signal)
        {
            for (int index = 0; index < signal.Cells.Count; index++)
            {
                m_Cleared.Add(signal.Cells[index]);
            }
        }

        private void OnBoosterApplied(ref BoosterAppliedSignal signal) => m_AppliedCount++;

        private void OnInputLockChanged(ref InputLockChangedSignal signal) => m_HasUnlocked = !signal.IsLocked;
    }
}
