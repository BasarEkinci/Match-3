namespace Match3.Model
{
    public readonly struct ClearedCell
    {
        public ClearedCell(GridPosition position, int wave)
        {
            Position = position;
            Wave = wave;
        }

        public GridPosition Position { get; }

        public int Wave { get; }
    }
}
