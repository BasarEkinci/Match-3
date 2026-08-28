using System.Collections.Generic;

namespace Match3.Model.Matching
{
    public interface IMatchFinder
    {
        IReadOnlyList<MatchGroup> FindMatches(Board board);
    }
}
