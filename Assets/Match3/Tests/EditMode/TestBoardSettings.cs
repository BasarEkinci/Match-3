using Match3.Model.Settings;

namespace Match3.Tests.EditMode
{
    public sealed class TestBoardSettings : IBoardSettings
    {
        private const int TileColorCount = 6;
        private const int MatchLength = 3;

        public TestBoardSettings(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public int ColorCount => TileColorCount;

        public int MinMatchLength => MatchLength;
    }
}
