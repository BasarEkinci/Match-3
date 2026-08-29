using Match3.Model.Settings;

namespace Match3.Tests.EditMode
{
    public sealed class TestBoosterSettings : IBoosterSettings
    {
        private const int ChargeThreshold = 100;
        private const int CarryLimit = 2;

        public int ScorePerBooster => ChargeThreshold;

        public int MaxCarried => CarryLimit;
    }
}
