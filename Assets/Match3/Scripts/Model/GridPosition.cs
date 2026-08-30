using System;

namespace Match3.Model
{
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }

        public static int Distance(GridPosition from, GridPosition to) =>
            Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y));

        public bool Equals(GridPosition other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
}
