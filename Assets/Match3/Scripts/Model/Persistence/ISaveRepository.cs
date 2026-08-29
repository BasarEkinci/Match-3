namespace Match3.Model.Persistence
{
    public interface ISaveRepository
    {
        bool HasSave { get; }

        void LoadBoard(Board board);

        int LoadScore();

        void Save(Board board, int score);

        void Clear();
    }
}
