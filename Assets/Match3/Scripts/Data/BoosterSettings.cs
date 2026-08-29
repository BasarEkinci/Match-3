using Match3.Model.Settings;
using UnityEngine;

namespace Match3.Data
{
    [CreateAssetMenu(fileName = AssetName, menuName = MenuName)]
    public sealed class BoosterSettings : ScriptableObject, IBoosterSettings
    {
        private const string AssetName = "BoosterSettings";
        private const string MenuName = "Match3/Booster Settings";

        private const int MinScorePerBooster = 100;
        private const int MaxScorePerBooster = 100000;
        private const int DefaultScorePerBooster = 2000;
        private const int MinCarried = 1;
        private const int MaxCarriedLimit = 9;
        private const int DefaultMaxCarried = 3;

        [SerializeField, Range(MinScorePerBooster, MaxScorePerBooster)]
        private int scorePerBooster = DefaultScorePerBooster;

        [SerializeField, Range(MinCarried, MaxCarriedLimit)]
        private int maxCarried = DefaultMaxCarried;

        public int ScorePerBooster => scorePerBooster;

        public int MaxCarried => maxCarried;
    }
}
