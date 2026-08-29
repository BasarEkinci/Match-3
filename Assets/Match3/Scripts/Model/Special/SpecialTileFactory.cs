using System.Collections.Generic;
using Match3.Model.Enums;

namespace Match3.Model.Special
{
    public static class SpecialTileFactory
    {
        private const int CenterDivisor = 2;

        public static SpecialTileType Resolve(MatchShape shape)
        {
            switch (shape)
            {
                case MatchShape.Line4Horizontal:
                    return SpecialTileType.HorizontalRocket;
                case MatchShape.Line4Vertical:
                    return SpecialTileType.VerticalRocket;
                case MatchShape.Corner:
                    return SpecialTileType.Bomb;
                case MatchShape.Line5:
                    return SpecialTileType.ColorBomb;
                default:
                    return SpecialTileType.None;
            }
        }

        public static GridPosition ResolveOrigin(MatchGroup group, GridPosition swapFrom, GridPosition swapTo)
        {
            IReadOnlyList<GridPosition> positions = group.Positions;
            for (int index = 0; index < positions.Count; index++)
            {
                if (positions[index].Equals(swapFrom) || positions[index].Equals(swapTo))
                {
                    return positions[index];
                }
            }

            return positions[positions.Count / CenterDivisor];
        }
    }
}
