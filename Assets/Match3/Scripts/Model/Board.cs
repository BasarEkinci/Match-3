using System;

namespace Match3.Model
{
    public sealed class Board
    {
        private readonly Tile[] m_Tiles;

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            m_Tiles = new Tile[width * height];
        }

        public int Width { get; }

        public int Height { get; }

        public bool Contains(GridPosition position) =>
            position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;

        public bool TryGet(GridPosition position, out Tile tile)
        {
            if (!Contains(position))
            {
                tile = Tile.Empty;
                return false;
            }

            tile = m_Tiles[ToIndex(position)];
            return true;
        }

        public void Set(GridPosition position, Tile tile)
        {
            m_Tiles[RequireIndex(position)] = tile;
        }

        public void Swap(GridPosition first, GridPosition second)
        {
            int firstIndex = RequireIndex(first);
            int secondIndex = RequireIndex(second);
            (m_Tiles[firstIndex], m_Tiles[secondIndex]) = (m_Tiles[secondIndex], m_Tiles[firstIndex]);
        }

        private int ToIndex(GridPosition position) => (position.Y * Width) + position.X;

        private int RequireIndex(GridPosition position)
        {
            if (!Contains(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            return ToIndex(position);
        }
    }
}
