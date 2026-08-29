using System;
using Match3.Model.Settings;

namespace Match3.Model.Scoring
{
    public sealed class ScoreModel
    {
        private const int FirstCascadeStep = 1;

        private readonly IScoreSettings m_Settings;

        public ScoreModel(IScoreSettings settings)
        {
            m_Settings = settings;
            Multiplier = settings.BaseMultiplier;
        }

        public int Total { get; private set; }

        public float Multiplier { get; private set; }

        public void Restore(int total)
        {
            Total = total;
            Multiplier = m_Settings.BaseMultiplier;
        }

        public int AddClearedTiles(int tileCount, int cascadeStep)
        {
            Multiplier = MultiplierAt(cascadeStep);
            return Add((int)(tileCount * m_Settings.PointsPerTile * Multiplier));
        }

        public int AddSpecialTileCreation() => Add(m_Settings.SpecialTileCreationBonus);

        public int AddSpecialCombination() => Add(m_Settings.SpecialCombinationBonus);

        private int Add(int delta)
        {
            Total += delta;
            return delta;
        }

        private float MultiplierAt(int cascadeStep)
        {
            return Math.Min(
                m_Settings.BaseMultiplier
                    + ((cascadeStep - FirstCascadeStep) * m_Settings.MultiplierPerCascadeStep),
                m_Settings.MaxMultiplier);
        }
    }
}
