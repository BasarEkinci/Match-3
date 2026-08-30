using Match3.Model;
using Match3.Model.Settings;
using UnityEngine;

namespace Match3.View
{
    public sealed class BoardGeometry
    {
        private const float CenterOffset = 0.5f;

        private readonly IBoardSettings m_Settings;

        public BoardGeometry(IBoardSettings settings)
        {
            m_Settings = settings;
        }

        public float CellSize => m_Settings.CellSize;

        public Vector2 BoardSize => new Vector2(m_Settings.Width, m_Settings.Height) * CellSize;

        public float SpawnHeight => m_Settings.Height * CellSize;

        public Vector3 ToWorld(GridPosition position) =>
            new Vector3(
                (position.X - ((m_Settings.Width - 1) * CenterOffset)) * CellSize,
                (position.Y - ((m_Settings.Height - 1) * CenterOffset)) * CellSize,
                0f);

        public bool TryToGrid(Vector3 world, out GridPosition position)
        {
            int x = Mathf.RoundToInt((world.x / CellSize) + ((m_Settings.Width - 1) * CenterOffset));
            int y = Mathf.RoundToInt((world.y / CellSize) + ((m_Settings.Height - 1) * CenterOffset));
            position = new GridPosition(x, y);
            return x >= 0 && x < m_Settings.Width && y >= 0 && y < m_Settings.Height;
        }
    }
}
