namespace Match3.Model.Settings
{
    public interface IBoardSettings
    {
        int Width { get; }

        int Height { get; }

        int ColorCount { get; }

        int MinMatchLength { get; }
    }
}
