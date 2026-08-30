using System;
using UnityEngine;

namespace Match3.View
{
    public sealed class BoardBackgroundView : IDisposable
    {
        private const string ObjectName = "BoardBackground";
        private const int SortingOrder = -1;

        private readonly GameObject m_Instance;

        public BoardBackgroundView(BoardGeometry geometry, Sprite backgroundSprite)
        {
            if (backgroundSprite == null)
            {
                throw new ArgumentNullException(nameof(backgroundSprite));
            }

            float spriteScale = geometry.CellSize / backgroundSprite.bounds.size.x;
            m_Instance = new GameObject(ObjectName);
            SpriteRenderer renderer = m_Instance.AddComponent<SpriteRenderer>();
            renderer.sprite = backgroundSprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.sortingOrder = SortingOrder;
            renderer.size = geometry.BoardSize / spriteScale;
            m_Instance.transform.localScale = Vector3.one * spriteScale;
        }

        public void Dispose()
        {
            if (m_Instance != null)
            {
                UnityEngine.Object.Destroy(m_Instance);
            }
        }
    }
}
