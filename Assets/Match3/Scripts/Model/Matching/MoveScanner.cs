using Match3.Model.Enums;

namespace Match3.Model.Matching
{
    public sealed class MoveScanner : IMoveScanner
    {
        private readonly IMatchFinder m_MatchFinder;

        public MoveScanner(IMatchFinder matchFinder)
        {
            m_MatchFinder = matchFinder;
        }

        public bool TryFindMove(Board board, out GridPosition from, out GridPosition to)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    board.TryGet(position, out Tile tile);
                    if (!tile.IsEmpty && tile.Special == SpecialTileType.ColorBomb)
                    {
                        from = position;
                        to = position;
                        return true;
                    }

                    GridPosition right = new GridPosition(x + 1, y);
                    GridPosition up = new GridPosition(x, y + 1);
                    if (CreatesMatch(board, position, right))
                    {
                        from = position;
                        to = right;
                        return true;
                    }

                    if (CreatesMatch(board, position, up))
                    {
                        from = position;
                        to = up;
                        return true;
                    }
                }
            }

            from = default;
            to = default;
            return false;
        }

        private bool CreatesMatch(Board board, GridPosition from, GridPosition to)
        {
            if (!board.Contains(to))
            {
                return false;
            }

            board.TryGet(from, out Tile source);
            board.TryGet(to, out Tile target);
            if (source.IsEmpty || target.IsEmpty || source.Color == target.Color)
            {
                return false;
            }

            board.Swap(from, to);
            bool matched = m_MatchFinder.FindMatches(board).Count > 0;
            board.Swap(from, to);
            return matched;
        }
    }
}
