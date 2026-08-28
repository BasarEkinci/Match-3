namespace Match3.Model
{
    public readonly struct TileSpawn
    {
        public TileSpawn(GridPosition position, Tile tile)
        {
            Position = position;
            Tile = tile;
        }

        public GridPosition Position { get; }

        public Tile Tile { get; }
    }
}
