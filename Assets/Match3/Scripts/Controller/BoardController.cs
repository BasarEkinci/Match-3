using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Generation;
using Match3.Model.Gravity;
using Match3.Model.Matching;
using Match3.Model.Persistence;
using Match3.Model.Settings;
using Match3.Model.Special;
using Match3.Signals;
using Match3.Core.MessagePipe.Pipes;

namespace Match3.Controller
{
    public sealed class BoardController : IDisposable
    {
        private const int AdjacentDistance = 1;
        private const int FirstCascadeStep = 1;
        private const int FirstWave = 0;

        private readonly GamePipe m_GamePipe;
        private readonly ProjectPipe m_ProjectPipe;
        private readonly IBoardGenerator m_Generator;
        private readonly IMatchFinder m_MatchFinder;
        private readonly IGravityResolver m_GravityResolver;
        private readonly IMoveScanner m_MoveScanner;
        private readonly Board m_Board;
        private readonly List<TileMove> m_Moves = new List<TileMove>();
        private readonly List<TileSpawn> m_Spawns = new List<TileSpawn>();
        private readonly List<ClearedCell> m_Cleared = new List<ClearedCell>();
        private readonly ChainResolver m_ChainResolver;
        private readonly SpecialCombinationResolver m_Combinations;
        private readonly ISaveRepository m_Save;
        private readonly List<ClearedCell> m_Seeds = new List<ClearedCell>();
        private readonly CancellationTokenSource m_Lifetime = new CancellationTokenSource();

        private CancellationTokenSource m_Round;

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
            SpecialCombinationResolver combinations,
            ISaveRepository save)
        {
            m_GamePipe = gamePipe;
            m_ProjectPipe = projectPipe;
            m_Generator = generator;
            m_MatchFinder = matchFinder;
            m_GravityResolver = gravityResolver;
            m_MoveScanner = moveScanner;
            m_ChainResolver = chainResolver;
            m_Combinations = combinations;
            m_Save = save;
            m_Board = new Board(settings.Width, settings.Height);

            m_ProjectPipe.SubscribeTo<RoundStartedSignal>(OnRoundStarted);
            m_GamePipe.SubscribeTo<SwapRequestedSignal>(OnSwapRequested);
            m_GamePipe.SubscribeTo<SpecialActivationRequestedSignal>(OnSpecialActivationRequested);
            m_GamePipe.SubscribeTo<BoardAnimationCompletedSignal>(OnAnimationCompleted);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            EndRound();
            m_Lifetime.Cancel();
            m_Lifetime.Dispose();
            m_ProjectPipe.UnsubscribeFrom<RoundStartedSignal>(OnRoundStarted);
            m_GamePipe.UnsubscribeFrom<SwapRequestedSignal>(OnSwapRequested);
            m_GamePipe.UnsubscribeFrom<SpecialActivationRequestedSignal>(OnSpecialActivationRequested);
            m_GamePipe.UnsubscribeFrom<BoardAnimationCompletedSignal>(OnAnimationCompleted);
        }

        private void OnRoundStarted(ref RoundStartedSignal signal)
        {
            EndRound();
            m_Round = CancellationTokenSource.CreateLinkedTokenSource(m_Lifetime.Token);
            if (signal.IsResumed)
            {
                m_Save.LoadBoard(m_Board);
            }
            else
            {
                m_Generator.Generate(m_Board);
            }

            m_State = BoardState.Idle;
            m_GamePipe.Raise(new BoardCreatedSignal(m_Board));
            m_GamePipe.Raise(new InputLockChangedSignal(false));
        }

        private void EndRound()
        {
            if (m_Round == null)
            {
                return;
            }

            m_Round.Cancel();
            m_Round.Dispose();
            m_Round = null;
        }

        private void OnSwapRequested(ref SwapRequestedSignal signal)
        {
            if (m_State != BoardState.Idle || m_Round == null)
            {
                return;
            }

            if (!IsSwappable(signal.From, signal.To))
            {
                m_GamePipe.Raise(new SwapRejectedSignal(signal.From, signal.To));
                return;
            }

            m_Seeds.Clear();
            bool isCombination = IsCombination(signal.From, signal.To);
            m_Board.Swap(signal.From, signal.To);
            if (!isCombination)
            {
                bool createsMatch = m_MatchFinder.FindMatches(m_Board).Count > 0;
                bool hasSpecial = TrySeed(signal.From) | TrySeed(signal.To);
                if (!createsMatch && !hasSpecial)
                {
                    m_Board.Swap(signal.From, signal.To);
                    m_GamePipe.Raise(new SwapRejectedSignal(signal.From, signal.To));
                    return;
                }
            }

            m_State = BoardState.Swapping;
            m_GamePipe.Raise(new InputLockChangedSignal(true));
            RunSwap(signal.From, signal.To, isCombination, m_Round.Token).Forget();
        }

        private void OnSpecialActivationRequested(ref SpecialActivationRequestedSignal signal)
        {
            if (m_State != BoardState.Idle || m_Round == null)
            {
                return;
            }

            m_Seeds.Clear();
            if (!TrySeed(signal.Position))
            {
                return;
            }

            m_State = BoardState.Resolving;
            m_GamePipe.Raise(new InputLockChangedSignal(true));
            ResolveCascade(signal.Position, signal.Position, m_Round.Token).Forget();
        }

        private bool TrySeed(GridPosition position)
        {
            m_Board.TryGet(position, out Tile tile);
            if (tile.IsEmpty || tile.Special == SpecialTileType.None)
            {
                return false;
            }

            m_Seeds.Add(new ClearedCell(position, FirstWave));
            return true;
        }

        private async UniTaskVoid RunSwap(
            GridPosition from,
            GridPosition to,
            bool isCombination,
            CancellationToken token)
        {
            UniTask swapAnimation = WaitForAnimation(token);
            m_GamePipe.Raise(new SwapAcceptedSignal(from, to));
            await swapAnimation;

            if (isCombination)
            {
                await ResolveCombination(from, to, token);
            }

            await ResolveCascade(from, to, token);
        }

        private async UniTask ResolveCascade(GridPosition from, GridPosition to, CancellationToken token)
        {
            int cascadeStep = FirstCascadeStep;
            while (true)
            {
                IReadOnlyList<MatchGroup> groups = m_MatchFinder.FindMatches(m_Board);
                if (groups.Count == 0)
                {
                    if (m_Seeds.Count == 0)
                    {
                        break;
                    }

                    groups = null;
                }
                else
                {
                    SeedGroups(groups);
                    m_GamePipe.Raise(new MatchesResolvedSignal(groups, cascadeStep));
                }

                m_State = BoardState.Resolving;
                m_ChainResolver.Collect(m_Board, m_Seeds, m_Cleared);
                m_Seeds.Clear();
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
            if (m_MoveScanner.TryFindMove(m_Board, out _, out _))
            {
                return;
            }

            await Shuffle(token);
        }

        private async UniTask Shuffle(CancellationToken token)
        {
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

        private async UniTask ResolveCombination(GridPosition from, GridPosition to, CancellationToken token)
        {
            m_Board.TryGet(to, out Tile first);
            m_Board.TryGet(from, out Tile second);
            if (!m_Combinations.TryResolve(m_Board, from, to, m_Seeds))
            {
                return;
            }

            m_Board.Set(from, Tile.Empty);
            m_Board.Set(to, Tile.Empty);
            m_GamePipe.Raise(new SpecialCombinationTriggeredSignal(first.Special, second.Special, to));

            Tile partner = ConvertiblePartner(first, second);
            if (partner.IsEmpty)
            {
                return;
            }

            ConvertTiles(partner.Color, partner.Special);
            UniTask conversionAnimation = WaitForAnimation(token);
            m_GamePipe.Raise(new SpecialConversionSignal(partner.Color, partner.Special));
            await conversionAnimation;
        }

        private static Tile ConvertiblePartner(Tile first, Tile second)
        {
            if (first.Special != SpecialTileType.ColorBomb && second.Special != SpecialTileType.ColorBomb)
            {
                return Tile.Empty;
            }

            Tile partner = first.Special == SpecialTileType.ColorBomb ? second : first;
            return partner.Special == SpecialTileType.None || partner.Special == SpecialTileType.ColorBomb
                ? Tile.Empty
                : partner;
        }

        private void ConvertTiles(TileColor color, SpecialTileType special)
        {
            for (int y = 0; y < m_Board.Height; y++)
            {
                for (int x = 0; x < m_Board.Width; x++)
                {
                    GridPosition position = new GridPosition(x, y);
                    m_Board.TryGet(position, out Tile tile);
                    if (tile.IsEmpty || tile.Color != color || tile.Special != SpecialTileType.None)
                    {
                        continue;
                    }

                    m_Board.Set(position, new Tile(color, special));
                }
            }
        }

        private void SeedGroups(IReadOnlyList<MatchGroup> groups)
        {
            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                IReadOnlyList<GridPosition> positions = groups[groupIndex].Positions;
                for (int index = 0; index < positions.Count; index++)
                {
                    m_Seeds.Add(new ClearedCell(positions[index], FirstWave));
                }
            }
        }

        private void Clear()
        {
            for (int index = 0; index < m_Cleared.Count; index++)
            {
                m_Board.Set(m_Cleared[index].Position, Tile.Empty);
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

        private bool IsSwappable(GridPosition from, GridPosition to) =>
            m_Board.Contains(from) && m_Board.Contains(to) &&
            Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y) == AdjacentDistance;
    }
}
