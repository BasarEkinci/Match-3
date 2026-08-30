using System;
using Match3.Model.Enums;
using UnityEngine;
using UnityEngine.Pool;

namespace Match3.View
{
    public sealed class TilePool : IDisposable
    {
        private const string ContainerName = "Tiles";
        private const string TileName = "Tile";
        private const int DefaultCapacity = 64;
        private const int MaxSize = 128;
        private const float HorizontalRocketRotation = -45f;
        private const float VerticalRocketRotation = 45f;
        private const float NoRotation = 0f;

        private readonly BoardGeometry m_Geometry;
        private readonly Sprite[] m_Sprites;
        private readonly Sprite[] m_SpecialSprites;
        private readonly Transform m_Container;
        private readonly ObjectPool<TileView> m_Pool;

        public TilePool(BoardGeometry geometry, Sprite[] sprites, Sprite[] specialSprites)
        {
            int colorCount = Enum.GetNames(typeof(TileColor)).Length;
            if (sprites == null || sprites.Length != colorCount)
            {
                throw new ArgumentException($"Expected {colorCount} tile sprites ordered by {nameof(TileColor)}.", nameof(sprites));
            }

            int specialCount = Enum.GetNames(typeof(SpecialTileType)).Length;
            if (specialSprites == null || specialSprites.Length != specialCount)
            {
                throw new ArgumentException($"Expected {specialCount} sprites ordered by {nameof(SpecialTileType)}.", nameof(specialSprites));
            }

            m_Geometry = geometry;
            m_Container = new GameObject(ContainerName).transform;
            m_Sprites = sprites;
            m_SpecialSprites = specialSprites;
            m_Pool = new ObjectPool<TileView>(CreateTile, ActivateTile, DeactivateTile, DestroyTile, true, DefaultCapacity, MaxSize);
        }

        public TileView Get(TileColor color, SpecialTileType special, Vector3 position)
        {
            TileView tile = m_Pool.Get();
            Sprite sprite = SpriteOf(color, special);
            tile.Bind(sprite, position, RotationOf(special), ScaleOf(sprite));
            return tile;
        }

        private float ScaleOf(Sprite sprite) => m_Geometry.CellSize / sprite.bounds.size.x;

        private Sprite SpriteOf(TileColor color, SpecialTileType special) =>
            special == SpecialTileType.None ? m_Sprites[(int)color] : m_SpecialSprites[(int)special];

        private static float RotationOf(SpecialTileType special)
        {
            switch (special)
            {
                case SpecialTileType.HorizontalRocket:
                    return HorizontalRocketRotation;
                case SpecialTileType.VerticalRocket:
                    return VerticalRocketRotation;
                default:
                    return NoRotation;
            }
        }

        public void Release(TileView tile)
        {
            m_Pool.Release(tile);
        }

        public void Dispose()
        {
            m_Pool.Dispose();
            if (m_Container != null)
            {
                UnityEngine.Object.Destroy(m_Container.gameObject);
            }
        }

        private TileView CreateTile()
        {
            GameObject instance = new GameObject(TileName);
            instance.transform.SetParent(m_Container, false);
            return instance.AddComponent<TileView>();
        }

        private static void ActivateTile(TileView tile) => tile.gameObject.SetActive(true);

        private static void DeactivateTile(TileView tile) => tile.gameObject.SetActive(false);

        private static void DestroyTile(TileView tile) => UnityEngine.Object.Destroy(tile.gameObject);
    }
}
