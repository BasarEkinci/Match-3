using System;
using System.Collections.Generic;
using Match3.Model.Enums;

namespace Match3.Model.Special
{
    public sealed class SpecialCombinationResolver
    {
        private const int BlastRadius = 1;
        private const int WideBlastRadius = 2;

        private delegate void Combination(
            Board board,
            GridPosition origin,
            Tile first,
            Tile second,
            List<GridPosition> cells);

        private readonly RocketEffect m_HorizontalRocket = new RocketEffect(true);
        private readonly RocketEffect m_VerticalRocket = new RocketEffect(false);
        private readonly BombEffect m_Blast = new BombEffect(BlastRadius);
        private readonly BombEffect m_WideBlast = new BombEffect(WideBlastRadius);
        private readonly Combination[,] m_Table;
        private readonly List<GridPosition> m_Buffer = new List<GridPosition>();

        public SpecialCombinationResolver()
        {
            int typeCount = Enum.GetNames(typeof(SpecialTileType)).Length;
            m_Table = new Combination[typeCount, typeCount];

            Register(SpecialTileType.HorizontalRocket, SpecialTileType.Bomb, TripleLine);
            Register(SpecialTileType.VerticalRocket, SpecialTileType.Bomb, TripleLine);
            Register(SpecialTileType.Bomb, SpecialTileType.Bomb, WideBlast);
            Register(SpecialTileType.ColorBomb, SpecialTileType.None, ColorSweep);
            Register(SpecialTileType.ColorBomb, SpecialTileType.HorizontalRocket, ColorToRockets);
            Register(SpecialTileType.ColorBomb, SpecialTileType.VerticalRocket, ColorToRockets);
            Register(SpecialTileType.ColorBomb, SpecialTileType.Bomb, ColorToBombs);
            Register(SpecialTileType.ColorBomb, SpecialTileType.ColorBomb, WholeBoard);
        }

        public bool Contains(SpecialTileType first, SpecialTileType second) =>
            m_Table[(int)first, (int)second] != null;

        public bool TryResolve(Board board, GridPosition from, GridPosition to, List<ClearedCell> cells)
        {
            if (!board.TryGet(from, out Tile first) || !board.TryGet(to, out Tile second))
            {
                return false;
            }

            Combination combination = m_Table[(int)first.Special, (int)second.Special];
            if (combination == null)
            {
                return false;
            }

            m_Buffer.Clear();
            m_Buffer.Add(from);
            m_Buffer.Add(to);
            combination(board, to, first, second, m_Buffer);
            for (int index = 0; index < m_Buffer.Count; index++)
            {
                cells.Add(new ClearedCell(m_Buffer[index], GridPosition.Distance(to, m_Buffer[index])));
            }

            return true;
        }

        private void Register(SpecialTileType first, SpecialTileType second, Combination combination)
        {
            m_Table[(int)first, (int)second] = combination;
            m_Table[(int)second, (int)first] = combination;
        }

        private void TripleLine(Board board, GridPosition origin, Tile first, Tile second, List<GridPosition> cells)
        {
            bool isVertical = IsVerticalPair(first, second);
            ISpecialTileEffect rocket = isVertical ? m_VerticalRocket : m_HorizontalRocket;
            for (int offset = -BlastRadius; offset <= BlastRadius; offset++)
            {
                GridPosition line = isVertical
                    ? new GridPosition(origin.X + offset, origin.Y)
                    : new GridPosition(origin.X, origin.Y + offset);
                rocket.Collect(board, line, first, cells);
            }
        }

        private void WideBlast(Board board, GridPosition origin, Tile first, Tile second, List<GridPosition> cells)
        {
            m_WideBlast.Collect(board, origin, first, cells);
        }

        private static void ColorSweep(
            Board board,
            GridPosition origin,
            Tile first,
            Tile second,
            List<GridPosition> cells)
        {
            TileColor color = Partner(first, second).Color;
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    board.TryGet(position, out Tile tile);
                    if (!tile.IsEmpty && tile.Color == color)
                    {
                        cells.Add(position);
                    }
                }
            }
        }

        private void ColorToRockets(Board board, GridPosition origin, Tile first, Tile second, List<GridPosition> cells)
        {
            Tile partner = Partner(first, second);
            ISpecialTileEffect rocket = partner.Special == SpecialTileType.VerticalRocket
                ? m_VerticalRocket
                : m_HorizontalRocket;
            CollectOverColor(board, partner.Color, rocket, partner, cells);
        }

        private void ColorToBombs(Board board, GridPosition origin, Tile first, Tile second, List<GridPosition> cells)
        {
            Tile partner = Partner(first, second);
            CollectOverColor(board, partner.Color, m_Blast, partner, cells);
        }

        private static void WholeBoard(
            Board board,
            GridPosition origin,
            Tile first,
            Tile second,
            List<GridPosition> cells)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    if (board.TryGet(position, out Tile tile) && !tile.IsEmpty)
                    {
                        cells.Add(position);
                    }
                }
            }
        }

        private static void CollectOverColor(
            Board board,
            TileColor color,
            ISpecialTileEffect effect,
            Tile source,
            List<GridPosition> cells)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    board.TryGet(position, out Tile tile);
                    if (!tile.IsEmpty && tile.Color == color)
                    {
                        effect.Collect(board, position, source, cells);
                    }
                }
            }
        }

        private static Tile Partner(Tile first, Tile second) =>
            first.Special == SpecialTileType.ColorBomb ? second : first;

        private static bool IsVerticalPair(Tile first, Tile second) =>
            first.Special == SpecialTileType.VerticalRocket || second.Special == SpecialTileType.VerticalRocket;
    }
}
