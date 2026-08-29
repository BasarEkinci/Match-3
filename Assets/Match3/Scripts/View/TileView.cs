using UnityEngine;

namespace Match3.View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TileView : MonoBehaviour
    {
        private SpriteRenderer m_Renderer;

        public Transform Transform => transform;

        public void Bind(Sprite sprite, Vector3 position)
        {
            m_Renderer.sprite = sprite;
            transform.position = position;
        }

        private void Awake()
        {
            m_Renderer = GetComponent<SpriteRenderer>();
        }
    }
}
