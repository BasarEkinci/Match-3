using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Match3.Model;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;
using UnityEngine;

namespace Match3.View
{
    public sealed class BoardView : IDisposable
    {
        private const float SwapDuration = 0.18f;
        private const float ClearDuration = 0.15f;
        private const float FallDuration = 0.22f;
        private const float ShuffleNoticeDuration = 0.4f;
        private const float HighlightStrength = 0.35f;
        private const float HighlightDuration = 0.3f;
        private const int HighlightFrequency = 6;

        private readonly GamePipe m_GamePipe;
        private readonly TilePool m_Pool;
        private readonly BoardGeometry m_Geometry;
        private readonly List<TileView> m_Clearing = new List<TileView>();
        private readonly CancellationTokenSource m_Lifetime = new CancellationTokenSource();

        private Board m_Board;
        private TileView[] m_Tiles;
        private bool m_IsDisposed;

        public BoardView(GamePipe gamePipe, TilePool pool, BoardGeometry geometry)
        {
            m_GamePipe = gamePipe;
            m_Pool = pool;
            m_Geometry = geometry;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<SwapAcceptedSignal>(OnSwapAccepted);
            m_GamePipe.SubscribeTo<SwapRejectedSignal>(OnSwapRejected);
            m_GamePipe.SubscribeTo<CellsClearedSignal>(OnCellsCleared);
            m_GamePipe.SubscribeTo<BoardRefilledSignal>(OnBoardRefilled);
            m_GamePipe.SubscribeTo<BoardShuffleStartedSignal>(OnShuffleStarted);
            m_GamePipe.SubscribeTo<BoardShuffleCompletedSignal>(OnShuffleCompleted);
            m_GamePipe.SubscribeTo<SpecialTileCreatedSignal>(OnSpecialTileCreated);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_Lifetime.Cancel();
            m_Lifetime.Dispose();
            m_GamePipe.UnsubscribeFrom<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.UnsubscribeFrom<SwapAcceptedSignal>(OnSwapAccepted);
            m_GamePipe.UnsubscribeFrom<SwapRejectedSignal>(OnSwapRejected);
            m_GamePipe.UnsubscribeFrom<CellsClearedSignal>(OnCellsCleared);
            m_GamePipe.UnsubscribeFrom<BoardRefilledSignal>(OnBoardRefilled);
            m_GamePipe.UnsubscribeFrom<BoardShuffleStartedSignal>(OnShuffleStarted);
            m_GamePipe.UnsubscribeFrom<BoardShuffleCompletedSignal>(OnShuffleCompleted);
            m_GamePipe.UnsubscribeFrom<SpecialTileCreatedSignal>(OnSpecialTileCreated);
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal)
        {
            m_Board = signal.Board;
            Rebuild();
        }

        private void OnShuffleCompleted(ref BoardShuffleCompletedSignal signal)
        {
            Rebuild();
        }

        private void OnSwapAccepted(ref SwapAcceptedSignal signal)
        {
            AnimateSwap(signal.From, signal.To).Forget();
        }

        private void OnSwapRejected(ref SwapRejectedSignal signal)
        {
            AnimateRejectedSwap(signal.From, signal.To).Forget();
        }

        private void OnCellsCleared(ref CellsClearedSignal signal)
        {
            AnimateClear(signal.Cells).Forget();
        }

        private void OnBoardRefilled(ref BoardRefilledSignal signal)
        {
            AnimateRefill(signal.Moves, signal.Spawns).Forget();
        }

        private void OnShuffleStarted(ref BoardShuffleStartedSignal signal)
        {
            AnimateShuffleNotice().Forget();
        }

        private void Rebuild()
        {
            ReleaseAll();
            m_Tiles = new TileView[m_Board.Width * m_Board.Height];
            for (int y = 0; y < m_Board.Height; y++)
            {
                for (int x = 0; x < m_Board.Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    m_Board.TryGet(position, out Tile tile);
                    m_Tiles[ToIndex(position)] = m_Pool.Get(tile.Color, m_Geometry.ToWorld(position));
                }
            }
        }

        private void ReleaseAll()
        {
            if (m_Tiles == null)
            {
                return;
            }

            for (int index = 0; index < m_Tiles.Length; index++)
            {
                if (m_Tiles[index] != null)
                {
                    m_Pool.Release(m_Tiles[index]);
                    m_Tiles[index] = null;
                }
            }
        }

        private async UniTaskVoid AnimateSwap(GridPosition from, GridPosition to)
        {
            int fromIndex = ToIndex(from);
            int toIndex = ToIndex(to);
            TileView landingOnFrom = m_Tiles[toIndex];
            TileView landingOnTo = m_Tiles[fromIndex];
            m_Tiles[fromIndex] = landingOnFrom;
            m_Tiles[toIndex] = landingOnTo;

            MoveTo(landingOnFrom, m_Geometry.ToWorld(from), SwapDuration);
            MoveTo(landingOnTo, m_Geometry.ToWorld(to), SwapDuration);
            await Wait(SwapDuration);

            m_GamePipe.Raise(new BoardAnimationCompletedSignal());
        }

        private async UniTaskVoid AnimateRejectedSwap(GridPosition from, GridPosition to)
        {
            TileView fromTile = m_Tiles[ToIndex(from)];
            TileView toTile = m_Tiles[ToIndex(to)];

            MoveTo(fromTile, m_Geometry.ToWorld(to), SwapDuration);
            MoveTo(toTile, m_Geometry.ToWorld(from), SwapDuration);
            await Wait(SwapDuration);

            MoveTo(fromTile, m_Geometry.ToWorld(from), SwapDuration);
            MoveTo(toTile, m_Geometry.ToWorld(to), SwapDuration);
        }

        private async UniTaskVoid AnimateClear(IReadOnlyList<GridPosition> cells)
        {
            m_Clearing.Clear();
            for (int index = 0; index < cells.Count; index++)
            {
                int cell = ToIndex(cells[index]);
                TileView tile = m_Tiles[cell];
                if (tile == null)
                {
                    continue;
                }

                m_Tiles[cell] = null;
                m_Clearing.Add(tile);
                ShrinkAway(tile);
            }

            await Wait(ClearDuration);

            for (int index = 0; index < m_Clearing.Count; index++)
            {
                m_Pool.Release(m_Clearing[index]);
            }

            m_Clearing.Clear();
            m_GamePipe.Raise(new BoardAnimationCompletedSignal());
        }

        private void OnSpecialTileCreated(ref SpecialTileCreatedSignal signal)
        {
            m_Board.TryGet(signal.Position, out Tile tile);
            TileView view = m_Pool.Get(tile.Color, m_Geometry.ToWorld(signal.Position));
            m_Tiles[ToIndex(signal.Position)] = view;
            Highlight(view);
        }

        private async UniTaskVoid AnimateRefill(IReadOnlyList<TileMove> moves, IReadOnlyList<TileSpawn> spawns)
        {
            for (int index = 0; index < moves.Count; index++)
            {
                int fromIndex = ToIndex(moves[index].From);
                int toIndex = ToIndex(moves[index].To);
                m_Tiles[toIndex] = m_Tiles[fromIndex];
                m_Tiles[fromIndex] = null;
                MoveTo(m_Tiles[toIndex], m_Geometry.ToWorld(moves[index].To), FallDuration);
            }

            for (int index = 0; index < spawns.Count; index++)
            {
                TileSpawn spawn = spawns[index];
                Vector3 target = m_Geometry.ToWorld(spawn.Position);
                Vector3 origin = target + (Vector3.up * m_Geometry.SpawnHeight);
                TileView tile = m_Pool.Get(spawn.Tile.Color, origin);
                m_Tiles[ToIndex(spawn.Position)] = tile;
                MoveTo(tile, target, FallDuration);
            }

            await Wait(FallDuration);

            m_GamePipe.Raise(new BoardAnimationCompletedSignal());
        }

        private async UniTaskVoid AnimateShuffleNotice()
        {
            await Wait(ShuffleNoticeDuration);

            m_GamePipe.Raise(new BoardAnimationCompletedSignal());
        }

        private UniTask Wait(float seconds) =>
            UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: m_Lifetime.Token);

        private static void MoveTo(TileView tile, Vector3 target, float duration)
        {
            LMotion.Create(tile.Transform.position, target, duration)
                .WithEase(Ease.OutQuad)
                .BindToPosition(tile.Transform)
                .AddTo(tile.gameObject);
        }

        private static void Highlight(TileView tile)
        {
            LMotion.Punch.Create(Vector3.one, Vector3.one * HighlightStrength, HighlightDuration)
                .WithFrequency(HighlightFrequency)
                .BindToLocalScale(tile.Transform)
                .AddTo(tile.gameObject);
        }

        private static void ShrinkAway(TileView tile)
        {
            LMotion.Create(Vector3.one, Vector3.zero, ClearDuration)
                .WithEase(Ease.InQuad)
                .BindToLocalScale(tile.Transform)
                .AddTo(tile.gameObject);
        }

        private int ToIndex(GridPosition position) => (position.Y * m_Board.Width) + position.X;
    }
}
