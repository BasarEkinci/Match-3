using System.Collections.Generic;
using Match3.Controller;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Scoring;
using Match3.Signals;
using NUnit.Framework;
using Match3.Core.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class ScoreControllerTests
    {
        private const int TileCount = 3;
        private const int SavedScore = 900;

        private GamePipe m_GamePipe;
        private ProjectPipe m_ProjectPipe;
        private ScoreController m_Controller;
        private FakeSaveRepository m_Save;
        private List<ScoreChangedSignal> m_Changes;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_Save = new FakeSaveRepository();
            m_Changes = new List<ScoreChangedSignal>();
            m_GamePipe.SubscribeTo<ScoreChangedSignal>(OnScoreChanged);
            m_Controller = new ScoreController(m_GamePipe, m_ProjectPipe, new ScoreModel(new TestScoreSettings()), m_Save);
        }

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
            m_ProjectPipe.Dispose();
        }

        [Test]
        public void MatchesRaiseScoreChanged()
        {
            m_GamePipe.Raise(new CellsClearedSignal(Cells(TileCount), 1));

            Assert.AreEqual(1, m_Changes.Count);
            Assert.AreEqual(30, m_Changes[0].Total);
            Assert.AreEqual(30, m_Changes[0].Delta);
            Assert.AreEqual(1f, m_Changes[0].Multiplier);
        }

        [Test]
        public void BonusSignalsAccumulateOnTop()
        {
            m_GamePipe.Raise(new CellsClearedSignal(Cells(TileCount), 1));
            m_GamePipe.Raise(new SpecialTileCreatedSignal(new GridPosition(0, 0), SpecialTileType.Bomb));
            m_GamePipe.Raise(new SpecialCombinationTriggeredSignal(
                SpecialTileType.Bomb,
                SpecialTileType.Bomb,
                new GridPosition(0, 0)));

            Assert.AreEqual(3, m_Changes.Count);
            Assert.AreEqual(630, m_Changes[2].Total);
            Assert.AreEqual(500, m_Changes[2].Delta);
        }

        [Test]
        public void NewRoundResetsTheScore()
        {
            m_GamePipe.Raise(new CellsClearedSignal(Cells(TileCount), 1));

            m_ProjectPipe.Raise(new RoundStartedSignal(false));

            Assert.AreEqual(0, m_Changes[m_Changes.Count - 1].Total);
            Assert.AreEqual(0, m_Changes[m_Changes.Count - 1].Delta);
        }

        [Test]
        public void ResumedRoundRestoresTheSavedScore()
        {
            m_Save.Save(new Board(1, 1), SavedScore);

            m_ProjectPipe.Raise(new RoundStartedSignal(true));

            Assert.AreEqual(SavedScore, m_Changes[m_Changes.Count - 1].Total);
        }

        [Test]
        public void DisposedControllerStopsScoring()
        {
            m_Controller.Dispose();

            m_GamePipe.Raise(new CellsClearedSignal(Cells(TileCount), 1));

            Assert.AreEqual(0, m_Changes.Count);
        }

        private static IReadOnlyList<ClearedCell> Cells(int count)
        {
            List<ClearedCell> cells = new List<ClearedCell>();
            for (int x = 0; x < count; x++)
            {
                cells.Add(new ClearedCell(new GridPosition(x, 0), 0));
            }

            return cells;
        }

        private void OnScoreChanged(ref ScoreChangedSignal signal) => m_Changes.Add(signal);
    }
}
