namespace Match3.Model.Matching
{
    public interface IMoveScanner
    {
        bool TryFindMove(Board board, out GridPosition from, out GridPosition to);
    }
}
