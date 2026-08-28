using Match3.Model.Settings;
using UnityEngine;

namespace Match3.Data
{
    [CreateAssetMenu(fileName = AssetName, menuName = MenuName)]
    public sealed class ScoreSettings : ScriptableObject, IScoreSettings
    {
        private const string AssetName = "ScoreSettings";
        private const string MenuName = "Match3/Score Settings";

        private const int MinPointsPerTile = 1;
        private const int MaxPointsPerTile = 1000;
        private const int DefaultPointsPerTile = 10;
        private const float MinMultiplierValue = 1f;
        private const float MaxMultiplierValue = 50f;
        private const float DefaultBaseMultiplier = 1f;
        private const float MinMultiplierStep = 0f;
        private const float MaxMultiplierStep = 5f;
        private const float DefaultMultiplierStep = 0.5f;
        private const float DefaultMaxMultiplier = 10f;
        private const int MinBonus = 0;
        private const int MaxBonus = 10000;
        private const int DefaultSpecialTileCreationBonus = 100;
        private const int DefaultSpecialCombinationBonus = 500;

        [SerializeField, Range(MinPointsPerTile, MaxPointsPerTile)]
        private int pointsPerTile = DefaultPointsPerTile;

        [SerializeField, Range(MinMultiplierValue, MaxMultiplierValue)]
        private float baseMultiplier = DefaultBaseMultiplier;

        [SerializeField, Range(MinMultiplierStep, MaxMultiplierStep)]
        private float multiplierPerCascadeStep = DefaultMultiplierStep;

        [SerializeField, Range(MinMultiplierValue, MaxMultiplierValue)]
        private float maxMultiplier = DefaultMaxMultiplier;

        [SerializeField, Range(MinBonus, MaxBonus)]
        private int specialTileCreationBonus = DefaultSpecialTileCreationBonus;

        [SerializeField, Range(MinBonus, MaxBonus)]
        private int specialCombinationBonus = DefaultSpecialCombinationBonus;

        public int PointsPerTile => pointsPerTile;

        public float BaseMultiplier => baseMultiplier;

        public float MultiplierPerCascadeStep => multiplierPerCascadeStep;

        public float MaxMultiplier => maxMultiplier;

        public int SpecialTileCreationBonus => specialTileCreationBonus;

        public int SpecialCombinationBonus => specialCombinationBonus;
    }
}
