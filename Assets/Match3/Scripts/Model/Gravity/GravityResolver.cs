using System;
using System.Collections.Generic;
using Match3.Model.Enums;
using Match3.Model.Settings;

namespace Match3.Model.Gravity
{
    public sealed class GravityResolver : IGravityResolver
    {
        private readonly IBoardSettings m_Settings;
        private readonly Random m_Random;

        public GravityResolver(IBoardSettings settings, Random random)
        {
            m_Settings = settings;
            m_Random = random;
        }

        public void Resolve(Board board, List<TileMove> moves, List<TileSpawn> spawns)
        {
            moves.Clear();
            spawns.Clear();

            for (int x = 0; x < board.Width; x++)
            {
                int landingY = CollapseColumn(board, x, moves);
                FillColumn(board, x, landingY, spawns);
            }
        }

        private static int CollapseColumn(Board board, int x, List<TileMove> moves)
        {
            int landingY = 0;
            for (int y = 0; y < board.Height; y++)
            {
                GridPosition source = new GridPosition(x, y);
                board.TryGet(source, out Tile tile);
                if (tile.IsEmpty)
                {
                    continue;
                }

                if (y != landingY)
                {
                    GridPosition target = new GridPosition(x, landingY);
                    board.Set(target, tile);
                    board.Set(source, Tile.Empty);
                    moves.Add(new TileMove(source, target));
                }

                landingY++;
            }

            return landingY;
        }

        private void FillColumn(Board board, int x, int landingY, List<TileSpawn> spawns)
        {
            for (int y = landingY; y < board.Height; y++)
            {
                GridPosition position = new GridPosition(x, y);
                Tile tile = new Tile((TileColor)m_Random.Next(m_Settings.ColorCount));
                board.Set(position, tile);
                spawns.Add(new TileSpawn(position, tile));
            }
        }
    }
}
