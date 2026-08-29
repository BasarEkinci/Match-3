using Match3.Controller;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Signals;
using NUnit.Framework;
using Syntac.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class SaveControllerTests
    {
        private const int Width = 2;
        private const int Height = 2;
        private const int Score = 750;
        private const int Delta = 50;
        private const float Multiplier = 1f;

        private GamePipe m_GamePipe;
        private ProjectPipe m_ProjectPipe;
        private FakeSaveRepository m_Repository;
        private SaveController m_Controller;
        private Board m_Board;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_Repository = new FakeSaveRepository();
            m_Controller = new SaveController(m_GamePipe, m_ProjectPipe, m_Repository);
            m_Board = new Board(Width, Height);
            m_Board.Set(new GridPosition(0, 0), new Tile(TileColor.Blue, SpecialTileType.Bomb));
        }

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
            m_ProjectPipe.Dispose();
        }

        [Test]
        public void SettledBoardIsSavedWithTheCurrentScore()
        {
            CreateBoard();
            m_GamePipe.Raise(new ScoreChangedSignal(Score, Delta, Multiplier));

            m_GamePipe.Raise(new InputLockChangedSignal(false));

            Assert.AreEqual(1, m_Repository.SaveCount);
            Assert.AreEqual(Score, m_Repository.LoadScore());
        }

        [Test]
        public void LockedInputDoesNotSave()
        {
            CreateBoard();

            m_GamePipe.Raise(new InputLockChangedSignal(true));

            Assert.AreEqual(0, m_Repository.SaveCount);
        }

        [Test]
        public void NothingIsSavedBeforeABoardExists()
        {
            m_GamePipe.Raise(new InputLockChangedSignal(false));

            Assert.AreEqual(0, m_Repository.SaveCount);
        }

        [Test]
        public void SavedBoardRestoresEveryTile()
        {
            CreateBoard();
            m_GamePipe.Raise(new InputLockChangedSignal(false));

            Board restored = new Board(Width, Height);
            m_Repository.LoadBoard(restored);

            restored.TryGet(new GridPosition(0, 0), out Tile tile);
            Assert.AreEqual(TileColor.Blue, tile.Color);
            Assert.AreEqual(SpecialTileType.Bomb, tile.Special);
        }

        [Test]
        public void EndingTheRoundClearsTheSave()
        {
            CreateBoard();
            m_GamePipe.Raise(new InputLockChangedSignal(false));

            m_ProjectPipe.Raise(new RoundEndedSignal(Score));

            Assert.AreEqual(1, m_Repository.ClearCount);
            Assert.IsFalse(m_Repository.HasSave);
        }

        private void CreateBoard()
        {
            m_GamePipe.Raise(new BoardCreatedSignal(m_Board));
        }
    }
}
