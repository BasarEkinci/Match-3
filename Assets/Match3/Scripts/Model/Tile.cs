using Match3.Model.Enums;

namespace Match3.Model
{
    public readonly struct Tile
    {
        public static readonly Tile Empty = default;

        private readonly bool m_Occupied;

        public Tile(TileColor color, SpecialTileType special = SpecialTileType.None)
        {
            Color = color;
            Special = special;
            m_Occupied = true;
        }

        public TileColor Color { get; }

        public SpecialTileType Special { get; }

        public bool IsEmpty => !m_Occupied;
    }
}
