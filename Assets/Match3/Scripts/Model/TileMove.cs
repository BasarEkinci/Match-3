namespace Match3.Model
{
    public readonly struct TileMove
    {
        public TileMove(GridPosition from, GridPosition to)
        {
            From = from;
            To = to;
        }

        public GridPosition From { get; }

        public GridPosition To { get; }
    }
}
