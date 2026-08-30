using System;
using Match3.Model.Enums;
using Match3.Model.Matching;
using Match3.Model.Settings;

namespace Match3.Model.Generation
{
    public sealed class BoardGenerator : IBoardGenerator
    {
        private readonly IBoardSettings m_Settings;
        private readonly MatchFinder m_MatchFinder;
        private readonly MoveScanner m_MoveScanner;
        private readonly Random m_Random;
        private readonly int[] m_ColorOrder;

        public BoardGenerator(
            IBoardSettings settings,
            MatchFinder matchFinder,
            MoveScanner moveScanner,
            Random random)
        {
            m_Settings = settings;
            m_MatchFinder = matchFinder;
            m_MoveScanner = moveScanner;
            m_Random = random;
            m_ColorOrder = new int[settings.ColorCount];
            for (int color = 0; color < m_ColorOrder.Length; color++)
            {
                m_ColorOrder[color] = color;
            }
        }

        public void Generate(Board board)
        {
            FillWithoutMatches(board);
            if (!m_MoveScanner.TryFindMove(board, out _, out _))
            {
                SwapUntilMoveExists(board);
            }
        }

        private void FillWithoutMatches(Board board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    board.Set(position, new Tile(PickSafeColor(board, position)));
                }
            }
        }

        private TileColor PickSafeColor(Board board, GridPosition position)
        {
            Shuffle();
            for (int candidate = 0; candidate < m_ColorOrder.Length; candidate++)
            {
                TileColor color = (TileColor)m_ColorOrder[candidate];
                if (!WouldMatch(board, position, color))
                {
                    return color;
                }
            }

            return (TileColor)m_ColorOrder[0];
        }

        private bool WouldMatch(Board board, GridPosition position, TileColor color)
        {
            int neighboursThatComplete = m_Settings.MinMatchLength - 1;
            return CountBackwards(board, position, color, 1, 0) >= neighboursThatComplete ||
                   CountBackwards(board, position, color, 0, 1) >= neighboursThatComplete;
        }

        private static int CountBackwards(Board board, GridPosition position, TileColor color, int stepX, int stepY)
        {
            int count = 0;
            GridPosition cursor = new GridPosition(position.X - stepX, position.Y - stepY);
            while (board.TryGet(cursor, out Tile tile) && !tile.IsEmpty && tile.Color == color)
            {
                count++;
                cursor = new GridPosition(cursor.X - stepX, cursor.Y - stepY);
            }

            return count;
        }

        private void Shuffle()
        {
            for (int index = m_ColorOrder.Length - 1; index > 0; index--)
            {
                int pick = m_Random.Next(index + 1);
                (m_ColorOrder[index], m_ColorOrder[pick]) = (m_ColorOrder[pick], m_ColorOrder[index]);
            }
        }

        private void SwapUntilMoveExists(Board board)
        {
            int cellCount = board.Width * board.Height;
            for (int first = 0; first < cellCount; first++)
            {
                for (int second = first + 1; second < cellCount; second++)
                {
                    GridPosition from = ToPosition(board, first);
                    GridPosition to = ToPosition(board, second);
                    board.Swap(from, to);
                    if (m_MatchFinder.FindMatches(board).Count == 0 && m_MoveScanner.TryFindMove(board, out _, out _))
                    {
                        return;
                    }

                    board.Swap(from, to);
                }
            }
        }

        private static GridPosition ToPosition(Board board, int cell) =>
            new GridPosition(cell % board.Width, cell / board.Width);
    }
}
