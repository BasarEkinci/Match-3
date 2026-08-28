namespace Match3.Model.Settings
{
    public interface IScoreSettings
    {
        int PointsPerTile { get; }

        float BaseMultiplier { get; }

        float MultiplierPerCascadeStep { get; }

        float MaxMultiplier { get; }

        int SpecialTileCreationBonus { get; }

        int SpecialCombinationBonus { get; }
    }
}
