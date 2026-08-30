using UnityEngine;

namespace Match3.View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TileView : MonoBehaviour
    {
        private SpriteRenderer m_Renderer;

        public Transform Transform => transform;

        public Vector3 BaseScale { get; private set; }

        public void Bind(Sprite sprite, Vector3 position, float rotation, float scale)
        {
            m_Renderer.sprite = sprite;
            BaseScale = Vector3.one * scale;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, rotation));
            transform.localScale = BaseScale;
        }

        private void Awake()
        {
            m_Renderer = GetComponent<SpriteRenderer>();
        }
    }
}
