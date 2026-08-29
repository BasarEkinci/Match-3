using System;
using Match3.Model.Enums;
using UnityEngine;
using UnityEngine.Pool;

namespace Match3.View
{
    public sealed class TilePool : IDisposable
    {
        private const string SpritePathPrefix = "Match3/Gems/T_Gem_";
        private const string ContainerName = "Tiles";
        private const string TileName = "Tile";
        private const int DefaultCapacity = 64;
        private const int MaxSize = 128;

        private readonly Sprite[] m_Sprites;
        private readonly Transform m_Container;
        private readonly ObjectPool<TileView> m_Pool;

        public TilePool()
        {
            m_Container = new GameObject(ContainerName).transform;
            m_Sprites = LoadSprites();
            m_Pool = new ObjectPool<TileView>(CreateTile, ActivateTile, DeactivateTile, DestroyTile, true, DefaultCapacity, MaxSize);
        }

        public TileView Get(TileColor color, Vector3 position)
        {
            TileView tile = m_Pool.Get();
            tile.Bind(m_Sprites[(int)color], position);
            return tile;
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

        private static Sprite[] LoadSprites()
        {
            string[] names = Enum.GetNames(typeof(TileColor));
            Sprite[] sprites = new Sprite[names.Length];
            for (int index = 0; index < names.Length; index++)
            {
                string path = SpritePathPrefix + names[index];
                sprites[index] = Resources.Load<Sprite>(path);
                if (sprites[index] == null)
                {
                    throw new NullReferenceException($"No tile sprite at Resources/{path}.");
                }
            }

            return sprites;
        }

        private TileView CreateTile()
        {
            GameObject instance = new GameObject(TileName);
            instance.transform.SetParent(m_Container, false);
            return instance.AddComponent<TileView>();
        }

        private static void ActivateTile(TileView tile)
        {
            tile.Transform.localScale = Vector3.one;
            tile.gameObject.SetActive(true);
        }

        private static void DeactivateTile(TileView tile) => tile.gameObject.SetActive(false);

        private static void DestroyTile(TileView tile) => UnityEngine.Object.Destroy(tile.gameObject);
    }
}
