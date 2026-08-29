using Match3.Model.Scoring;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class ScoreModelTests
    {
        private const int TileCount = 3;

        private TestScoreSettings m_Settings;
        private ScoreModel m_Score;

        [SetUp]
        public void SetUp()
        {
            m_Settings = new TestScoreSettings();
            m_Score = new ScoreModel(m_Settings);
        }

        [Test]
        public void SingleMatchScoresBaseMultiplier()
        {
            int delta = m_Score.AddClearedTiles(TileCount, 1);

            Assert.AreEqual(30, delta);
            Assert.AreEqual(30, m_Score.Total);
            Assert.AreEqual(1f, m_Score.Multiplier);
        }

        [Test]
        public void CascadeStepRaisesMultiplier()
        {
            m_Score.AddClearedTiles(TileCount, 3);

            Assert.AreEqual(2f, m_Score.Multiplier);
            Assert.AreEqual(60, m_Score.Total);
        }

        [Test]
        public void MultiplierIsCappedAtMaximum()
        {
            m_Score.AddClearedTiles(TileCount, 20);

            Assert.AreEqual(m_Settings.MaxMultiplier, m_Score.Multiplier);
        }

        [Test]
        public void FiveStepChainAccumulatesRisingMultipliers()
        {
            for (int step = 1; step <= 5; step++)
            {
                m_Score.AddClearedTiles(TileCount, step);
            }

            Assert.AreEqual(30 + 45 + 60 + 75 + 90, m_Score.Total);
        }

        [Test]
        public void EveryClearedTileScores()
        {
            int delta = m_Score.AddClearedTiles(TileCount + 4, 1);

            Assert.AreEqual(70, delta);
        }

        [Test]
        public void BonusesAddFlatAmounts()
        {
            m_Score.AddClearedTiles(TileCount, 1);

            Assert.AreEqual(m_Settings.SpecialTileCreationBonus, m_Score.AddSpecialTileCreation());
            Assert.AreEqual(m_Settings.SpecialCombinationBonus, m_Score.AddSpecialCombination());
            Assert.AreEqual(30 + 100 + 500, m_Score.Total);
        }
    }
}
