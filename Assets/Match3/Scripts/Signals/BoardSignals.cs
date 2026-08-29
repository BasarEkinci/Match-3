using System.Collections.Generic;
using Match3.Model;
using Match3.Model.Enums;
using Syntac.Signals;

namespace Match3.Signals
{
    public readonly struct BoardCreatedSignal : ISignal
    {
        public BoardCreatedSignal(Board board)
        {
            Board = board;
        }

        public Board Board { get; }
    }

    public readonly struct TileDragSignal : ISignal
    {
        public TileDragSignal(GridPosition origin, GridDirection direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public GridPosition Origin { get; }

        public GridDirection Direction { get; }
    }

    public readonly struct SwapRequestedSignal : ISignal
    {
        public SwapRequestedSignal(GridPosition from, GridPosition to)
        {
            From = from;
            To = to;
        }

        public GridPosition From { get; }

        public GridPosition To { get; }
    }

    public readonly struct SwapAcceptedSignal : ISignal
    {
        public SwapAcceptedSignal(GridPosition from, GridPosition to)
        {
            From = from;
            To = to;
        }

        public GridPosition From { get; }

        public GridPosition To { get; }
    }

    public readonly struct SwapRejectedSignal : ISignal
    {
        public SwapRejectedSignal(GridPosition from, GridPosition to)
        {
            From = from;
            To = to;
        }

        public GridPosition From { get; }

        public GridPosition To { get; }
    }

    public readonly struct MatchesResolvedSignal : ISignal
    {
        public MatchesResolvedSignal(IReadOnlyList<MatchGroup> groups, int cascadeStep)
        {
            Groups = groups;
            CascadeStep = cascadeStep;
        }

        public IReadOnlyList<MatchGroup> Groups { get; }

        public int CascadeStep { get; }
    }

    public readonly struct CellsClearedSignal : ISignal
    {
        public CellsClearedSignal(IReadOnlyList<GridPosition> cells, int cascadeStep)
        {
            Cells = cells;
            CascadeStep = cascadeStep;
        }

        public IReadOnlyList<GridPosition> Cells { get; }

        public int CascadeStep { get; }
    }

    public readonly struct BoardRefilledSignal : ISignal
    {
        public BoardRefilledSignal(IReadOnlyList<TileMove> moves, IReadOnlyList<TileSpawn> spawns)
        {
            Moves = moves;
            Spawns = spawns;
        }

        public IReadOnlyList<TileMove> Moves { get; }

        public IReadOnlyList<TileSpawn> Spawns { get; }
    }

    public readonly struct BoardAnimationCompletedSignal : ISignal
    {
    }

    public readonly struct BoardShuffleStartedSignal : ISignal
    {
    }

    public readonly struct BoardShuffleCompletedSignal : ISignal
    {
    }

    public readonly struct InputLockChangedSignal : ISignal
    {
        public InputLockChangedSignal(bool isLocked)
        {
            IsLocked = isLocked;
        }

        public bool IsLocked { get; }
    }

    public readonly struct SpecialTileCreatedSignal : ISignal
    {
        public SpecialTileCreatedSignal(GridPosition position, SpecialTileType type)
        {
            Position = position;
            Type = type;
        }

        public GridPosition Position { get; }

        public SpecialTileType Type { get; }
    }

    public readonly struct SpecialCombinationTriggeredSignal : ISignal
    {
        public SpecialCombinationTriggeredSignal(SpecialTileType first, SpecialTileType second, GridPosition origin)
        {
            First = first;
            Second = second;
            Origin = origin;
        }

        public SpecialTileType First { get; }

        public SpecialTileType Second { get; }

        public GridPosition Origin { get; }
    }
}
