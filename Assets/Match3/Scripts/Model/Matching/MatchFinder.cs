using System;
using System.Collections.Generic;
using Match3.Model.Enums;
using Match3.Model.Settings;

namespace Match3.Model.Matching
{
    public sealed class MatchFinder
    {
        private const int NoRun = -1;
        private const int Line4Length = 4;

        private readonly IBoardSettings m_Settings;
        private readonly List<Run> m_Runs = new List<Run>();
        private readonly List<int> m_RunRoots = new List<int>();

        public MatchFinder(IBoardSettings settings)
        {
            m_Settings = settings;
        }

        public IReadOnlyList<MatchGroup> FindMatches(Board board)
        {
            m_Runs.Clear();
            CollectRuns(board, true);
            CollectRuns(board, false);

            if (m_Runs.Count == 0)
            {
                return Array.Empty<MatchGroup>();
            }

            MergeIntersectingRuns(board);
            return BuildGroups(board);
        }

        private void CollectRuns(Board board, bool horizontal)
        {
            int laneCount = horizontal ? board.Height : board.Width;
            int laneLength = horizontal ? board.Width : board.Height;

            for (int lane = 0; lane < laneCount; lane++)
            {
                int runLength = 0;
                TileColor runColor = default;

                for (int offset = 0; offset < laneLength; offset++)
                {
                    board.TryGet(ToPosition(lane, offset, horizontal), out Tile tile);
                    bool isMatchable = IsMatchable(tile);
                    bool continuesRun = runLength > 0 && isMatchable && tile.Color == runColor;
                    if (continuesRun)
                    {
                        runLength++;
                        continue;
                    }

                    AddRun(lane, offset - runLength, runLength, runColor, horizontal);
                    runLength = isMatchable ? 1 : 0;
                    runColor = tile.Color;
                }

                AddRun(lane, laneLength - runLength, runLength, runColor, horizontal);
            }
        }

        private void AddRun(int lane, int start, int length, TileColor color, bool horizontal)
        {
            if (length < m_Settings.MinMatchLength)
            {
                return;
            }

            m_Runs.Add(new Run(lane, start, length, color, horizontal));
        }

        private void MergeIntersectingRuns(Board board)
        {
            m_RunRoots.Clear();
            for (int i = 0; i < m_Runs.Count; i++)
            {
                m_RunRoots.Add(i);
            }

            int[] runOfCell = new int[board.Width * board.Height];
            for (int i = 0; i < runOfCell.Length; i++)
            {
                runOfCell[i] = NoRun;
            }

            for (int runIndex = 0; runIndex < m_Runs.Count; runIndex++)
            {
                Run run = m_Runs[runIndex];
                for (int offset = 0; offset < run.Length; offset++)
                {
                    GridPosition position = ToPosition(run.Lane, run.Start + offset, run.Horizontal);
                    int cell = (position.Y * board.Width) + position.X;
                    if (runOfCell[cell] == NoRun)
                    {
                        runOfCell[cell] = runIndex;
                        continue;
                    }

                    Union(runIndex, runOfCell[cell]);
                }
            }
        }

        private List<MatchGroup> BuildGroups(Board board)
        {
            List<MatchGroup> groups = new List<MatchGroup>();
            bool[] visitedCell = new bool[board.Width * board.Height];

            for (int rootCandidate = 0; rootCandidate < m_Runs.Count; rootCandidate++)
            {
                if (Find(rootCandidate) != rootCandidate)
                {
                    continue;
                }

                List<GridPosition> positions = new List<GridPosition>();
                int mergedRunCount = 0;
                Run longestRun = default;

                for (int runIndex = 0; runIndex < m_Runs.Count; runIndex++)
                {
                    if (Find(runIndex) != rootCandidate)
                    {
                        continue;
                    }

                    Run run = m_Runs[runIndex];
                    if (mergedRunCount == 0 || run.Length > longestRun.Length)
                    {
                        longestRun = run;
                    }

                    mergedRunCount++;
                    for (int offset = 0; offset < run.Length; offset++)
                    {
                        GridPosition position = ToPosition(run.Lane, run.Start + offset, run.Horizontal);
                        int cell = (position.Y * board.Width) + position.X;
                        if (visitedCell[cell])
                        {
                            continue;
                        }

                        visitedCell[cell] = true;
                        positions.Add(position);
                    }
                }

                groups.Add(new MatchGroup(positions, longestRun.Color, ResolveShape(longestRun, mergedRunCount)));
            }

            return groups;
        }

        private MatchShape ResolveShape(Run run, int mergedRunCount)
        {
            if (run.Length > Line4Length)
            {
                return MatchShape.Line5;
            }

            if (mergedRunCount > 1)
            {
                return MatchShape.Corner;
            }

            if (run.Length == m_Settings.MinMatchLength)
            {
                return MatchShape.Line3;
            }

            return run.Horizontal ? MatchShape.Line4Horizontal : MatchShape.Line4Vertical;
        }

        private int Find(int runIndex)
        {
            while (m_RunRoots[runIndex] != runIndex)
            {
                m_RunRoots[runIndex] = m_RunRoots[m_RunRoots[runIndex]];
                runIndex = m_RunRoots[runIndex];
            }

            return runIndex;
        }

        private void Union(int first, int second)
        {
            int firstRoot = Find(first);
            int secondRoot = Find(second);
            if (firstRoot == secondRoot)
            {
                return;
            }

            m_RunRoots[firstRoot > secondRoot ? firstRoot : secondRoot] =
                firstRoot < secondRoot ? firstRoot : secondRoot;
        }

        private static bool IsMatchable(Tile tile) =>
            !tile.IsEmpty && tile.Special != SpecialTileType.ColorBomb;

        private static GridPosition ToPosition(int lane, int offset, bool horizontal) =>
            horizontal ? new GridPosition(offset, lane) : new GridPosition(lane, offset);

        private readonly struct Run
        {
            public Run(int lane, int start, int length, TileColor color, bool horizontal)
            {
                Lane = lane;
                Start = start;
                Length = length;
                Color = color;
                Horizontal = horizontal;
            }

            public int Lane { get; }

            public int Start { get; }

            public int Length { get; }

            public TileColor Color { get; }

            public bool Horizontal { get; }
        }
    }
}
