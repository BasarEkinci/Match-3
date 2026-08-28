using System.Collections.Generic;
using Match3.Model.Enums;

namespace Match3.Model
{
    public readonly struct MatchGroup
    {
        public MatchGroup(IReadOnlyList<GridPosition> positions, TileColor color, MatchShape shape)
        {
            Positions = positions;
            Color = color;
            Shape = shape;
        }

        public IReadOnlyList<GridPosition> Positions { get; }

        public TileColor Color { get; }

        public MatchShape Shape { get; }
    }
}
