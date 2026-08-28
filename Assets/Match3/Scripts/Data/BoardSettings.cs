using Match3.Model.Settings;
using UnityEngine;

namespace Match3.Data
{
    [CreateAssetMenu(fileName = AssetName, menuName = MenuName)]
    public sealed class BoardSettings : ScriptableObject, IBoardSettings
    {
        private const string AssetName = "BoardSettings";
        private const string MenuName = "Match3/Board Settings";

        private const int MinBoardSize = 3;
        private const int MaxBoardSize = 16;
        private const int DefaultBoardSize = 8;
        private const int MinColorCount = 3;
        private const int TileColorCount = 6;
        private const int MinAllowedMatchLength = 3;
        private const int MaxAllowedMatchLength = 5;
        private const int DefaultMinMatchLength = 3;

        [SerializeField, Range(MinBoardSize, MaxBoardSize)]
        private int width = DefaultBoardSize;

        [SerializeField, Range(MinBoardSize, MaxBoardSize)]
        private int height = DefaultBoardSize;

        [SerializeField, Range(MinColorCount, TileColorCount)]
        private int colorCount = TileColorCount;

        [SerializeField, Range(MinAllowedMatchLength, MaxAllowedMatchLength)]
        private int minMatchLength = DefaultMinMatchLength;

        public int Width => width;

        public int Height => height;

        public int ColorCount => colorCount;

        public int MinMatchLength => minMatchLength;
    }
}
