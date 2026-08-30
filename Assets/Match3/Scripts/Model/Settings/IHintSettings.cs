namespace Match3.Model.Settings
{
    public interface IHintSettings
    {
        float IdleSeconds { get; }

        float RepeatSeconds { get; }

        float HighlightStrength { get; }

        float HighlightDuration { get; }

        int HighlightFrequency { get; }
    }
}
