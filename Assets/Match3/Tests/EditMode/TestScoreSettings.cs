using Match3.Model.Settings;

namespace Match3.Tests.EditMode
{
    public sealed class TestScoreSettings : IScoreSettings
    {
        private const int TilePoints = 10;
        private const float StartMultiplier = 1f;
        private const float MultiplierStep = 0.5f;
        private const float MultiplierCap = 3f;
        private const int CreationBonus = 100;
        private const int CombinationBonus = 500;

        public int PointsPerTile => TilePoints;

        public float BaseMultiplier => StartMultiplier;

        public float MultiplierPerCascadeStep => MultiplierStep;

        public float MaxMultiplier => MultiplierCap;

        public int SpecialTileCreationBonus => CreationBonus;

        public int SpecialCombinationBonus => CombinationBonus;
    }
}
