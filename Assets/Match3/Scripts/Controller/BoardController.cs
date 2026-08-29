using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Generation;
using Match3.Model.Gravity;
using Match3.Model.Matching;
using Match3.Model.Settings;
using Match3.Model.Special;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;

namespace Match3.Controller
{
    public sealed class BoardController : IDisposable
    {
        private const int AdjacentDistance = 1;
        private const int FirstCascadeStep = 1;

        private readonly GamePipe m_GamePipe;
        private readonly ProjectPipe m_ProjectPipe;
        private readonly IBoardGenerator m_Generator;
        private readonly IMatchFinder m_MatchFinder;
        private readonly IGravityResolver m_GravityResolver;
        private readonly IMoveScanner m_MoveScanner;
        private readonly Board m_Board;
        private readonly List<TileMove> m_Moves = new List<TileMove>();
        private readonly List<TileSpawn> m_Spawns = new List<TileSpawn>();
        private readonly List<GridPosition> m_Cleared = new List<GridPosition>();
        private readonly ChainResolver m_ChainResolver;
        private readonly SpecialCombinationResolver m_Combinations;
        private readonly List<GridPosition> m_Seeds = new List<GridPosition>();
        private readonly CancellationTokenSource m_Lifetime = new CancellationTokenSource();

        private BoardState m_State;
        private bool m_IsDisposed;
        private UniTaskCompletionSource m_AnimationCompletion;

        public BoardController(
            GamePipe gamePipe,
            ProjectPipe projectPipe,
            IBoardSettings settings,
            IBoardGenerator generator,
            IMatchFinder matchFinder,
            IGravityResolver gravityResolver,
            IMoveScanner moveScanner,
            ChainResolver chainResolver,
            SpecialCombinationResolver combinations)
        {
            m_GamePipe = gamePipe;
            m_ProjectPipe = projectPipe;
            m_Generator = generator;
            m_MatchFinder = matchFinder;
            m_GravityResolver = gravityResolver;
            m_MoveScanner = moveScanner;
            m_ChainResolver = chainResolver;
            m_Combinations = combinations;
            m_Board = new Board(settings.Width, settings.Height);

            m_ProjectPipe.SubscribeTo<RoundStartedSignal>(OnRoundStarted);
            m_GamePipe.SubscribeTo<SwapRequestedSignal>(OnSwapRequested);
            m_GamePipe.SubscribeTo<BoardAnimationCompletedSignal>(OnAnimationCompleted);
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
            m_ProjectPipe.UnsubscribeFrom<RoundStartedSignal>(OnRoundStarted);
            m_GamePipe.UnsubscribeFrom<SwapRequestedSignal>(OnSwapRequested);
            m_GamePipe.UnsubscribeFrom<BoardAnimationCompletedSignal>(OnAnimationCompleted);
        }

        private void OnRoundStarted(ref RoundStartedSignal signal)
        {
            m_Generator.Generate(m_Board);
            m_State = BoardState.Idle;
            m_GamePipe.Raise(new BoardCreatedSignal(m_Board));
        }

        private void OnSwapRequested(ref SwapRequestedSignal signal)
        {
            if (m_State != BoardState.Idle)
            {
                return;
            }

            if (!IsSwappable(signal.From, signal.To))
            {
                m_GamePipe.Raise(new SwapRejectedSignal(signal.From, signal.To));
                return;
            }

            bool isCombination = IsCombination(signal.From, signal.To);
            if (isCombination)
            {
                m_Board.Swap(signal.From, signal.To);
            }
            else if (!CreatesMatch(signal.From, signal.To))
            {
                m_GamePipe.Raise(new SwapRejectedSignal(signal.From, signal.To));
                return;
            }

            m_State = BoardState.Swapping;
            m_GamePipe.Raise(new InputLockChangedSignal(true));
            RunCascade(signal.From, signal.To, isCombination, m_Lifetime.Token).Forget();
        }

        private async UniTaskVoid RunCascade(
            GridPosition from,
            GridPosition to,
            bool isCombination,
            CancellationToken token)
        {
            UniTask swapAnimation = WaitForAnimation(token);
            m_GamePipe.Raise(new SwapAcceptedSignal(from, to));
            await swapAnimation;

            int cascadeStep = FirstCascadeStep;
            while (true)
            {
                IReadOnlyList<MatchGroup> groups = null;
                if (isCombination)
                {
                    isCombination = false;
                    if (!SeedCombination(from, to))
                    {
                        break;
                    }
                }
                else
                {
                    groups = m_MatchFinder.FindMatches(m_Board);
                    if (groups.Count == 0)
                    {
                        break;
                    }

                    SeedGroups(groups);
                    m_GamePipe.Raise(new MatchesResolvedSignal(groups, cascadeStep));
                }

                m_State = BoardState.Resolving;
                m_ChainResolver.Collect(m_Board, m_Seeds, m_Cleared);
                UniTask clearAnimation = WaitForAnimation(token);
                m_GamePipe.Raise(new CellsClearedSignal(m_Cleared, cascadeStep));
                Clear();
                if (groups != null)
                {
                    CreateSpecialTiles(groups, from, to);
                }

                await clearAnimation;

                m_State = BoardState.Refilling;
                m_GravityResolver.Resolve(m_Board, m_Moves, m_Spawns);
                UniTask refillAnimation = WaitForAnimation(token);
                m_GamePipe.Raise(new BoardRefilledSignal(m_Moves, m_Spawns));
                await refillAnimation;

                cascadeStep++;
            }

            await EnsurePlayableBoard(token);

            m_State = BoardState.Idle;
            m_GamePipe.Raise(new InputLockChangedSignal(false));
        }

        private async UniTask EnsurePlayableBoard(CancellationToken token)
        {
            if (m_MoveScanner.HasAnyMove(m_Board))
            {
                return;
            }

            m_State = BoardState.Shuffling;
            UniTask noticeAnimation = WaitForAnimation(token);
            m_GamePipe.Raise(new BoardShuffleStartedSignal());
            await noticeAnimation;

            m_Generator.Generate(m_Board);
            m_GamePipe.Raise(new BoardShuffleCompletedSignal());
        }

        private void CreateSpecialTiles(IReadOnlyList<MatchGroup> groups, GridPosition swapFrom, GridPosition swapTo)
        {
            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                MatchGroup group = groups[groupIndex];
                SpecialTileType type = SpecialTileFactory.Resolve(group.Shape);
                if (type == SpecialTileType.None)
                {
                    continue;
                }

                GridPosition origin = SpecialTileFactory.ResolveOrigin(group, swapFrom, swapTo);
                m_Board.Set(origin, new Tile(group.Color, type));
                m_GamePipe.Raise(new SpecialTileCreatedSignal(origin, type));
            }
        }

        private bool IsCombination(GridPosition from, GridPosition to)
        {
            m_Board.TryGet(from, out Tile first);
            m_Board.TryGet(to, out Tile second);
            return m_Combinations.Contains(first.Special, second.Special);
        }

        private bool SeedCombination(GridPosition from, GridPosition to)
        {
            m_Seeds.Clear();
            m_Board.TryGet(to, out Tile first);
            m_Board.TryGet(from, out Tile second);
            if (!m_Combinations.TryResolve(m_Board, from, to, m_Seeds))
            {
                return false;
            }

            m_GamePipe.Raise(new SpecialCombinationTriggeredSignal(first.Special, second.Special, to));
            return true;
        }

        private void SeedGroups(IReadOnlyList<MatchGroup> groups)
        {
            m_Seeds.Clear();
            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                IReadOnlyList<GridPosition> positions = groups[groupIndex].Positions;
                for (int index = 0; index < positions.Count; index++)
                {
                    m_Seeds.Add(positions[index]);
                }
            }
        }

        private void Clear()
        {
            for (int index = 0; index < m_Cleared.Count; index++)
            {
                m_Board.Set(m_Cleared[index], Tile.Empty);
            }
        }

        private UniTask WaitForAnimation(CancellationToken token)
        {
            m_AnimationCompletion = new UniTaskCompletionSource();
            return m_AnimationCompletion.Task.AttachExternalCancellation(token);
        }

        private void OnAnimationCompleted(ref BoardAnimationCompletedSignal signal)
        {
            m_AnimationCompletion?.TrySetResult();
        }

        private bool CreatesMatch(GridPosition from, GridPosition to)
        {
            m_Board.Swap(from, to);
            if (m_MatchFinder.FindMatches(m_Board).Count > 0)
            {
                return true;
            }

            m_Board.Swap(from, to);
            return false;
        }

        private bool IsSwappable(GridPosition from, GridPosition to) =>
            m_Board.Contains(from) && m_Board.Contains(to) &&
            Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y) == AdjacentDistance;
    }
}
