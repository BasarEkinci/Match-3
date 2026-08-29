using System;
using System.Collections.Generic;
using Match3.Model.Enums;

namespace Match3.Model.Special
{
    public sealed class SpecialTileEffects
    {
        private const int BombRadius = 1;

        private readonly ISpecialTileEffect[] m_Effects;

        public SpecialTileEffects()
        {
            m_Effects = new ISpecialTileEffect[Enum.GetNames(typeof(SpecialTileType)).Length];
            m_Effects[(int)SpecialTileType.HorizontalRocket] = new RocketEffect(true);
            m_Effects[(int)SpecialTileType.VerticalRocket] = new RocketEffect(false);
            m_Effects[(int)SpecialTileType.Bomb] = new BombEffect(BombRadius);
            m_Effects[(int)SpecialTileType.ColorBomb] = new ColorBombEffect();
        }

        public bool TryCollect(Board board, GridPosition origin, List<GridPosition> cells)
        {
            if (!board.TryGet(origin, out Tile tile) || tile.IsEmpty)
            {
                return false;
            }

            ISpecialTileEffect effect = m_Effects[(int)tile.Special];
            if (effect == null)
            {
                return false;
            }

            effect.Collect(board, origin, tile, cells);
            return true;
        }
    }
}
