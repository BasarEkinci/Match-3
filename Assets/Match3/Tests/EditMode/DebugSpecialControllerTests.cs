using System;
using Match3.Controller;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Signals;
using NUnit.Framework;
using Match3.Core.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class DebugSpecialControllerTests
    {
        private const int Width = 4;
        private const int Height = 4;
        private const int RandomSeed = 7;

        private GamePipe m_GamePipe;
        private DebugSpecialController m_Controller;
        private Board m_Board;
        private int m_CreatedCount;
        private GridPosition m_CreatedPosition;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_Controller = new DebugSpecialController(m_GamePipe, new Random(RandomSeed));
            m_Board = new Board(Width, Height);
            m_CreatedCount = 0;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    m_Board.Set(new GridPosition(x, y), new Tile(TileColor.Red));
                }
            }

            m_GamePipe.SubscribeTo<SpecialTileCreatedSignal>(OnSpecialTileCreated);
            m_GamePipe.Raise(new BoardCreatedSignal(m_Board));
        }

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
        }

        [Test]
        public void RequestPlacesExactlyOneSpecialOnTheReportedCell()
        {
            m_GamePipe.Raise(new DebugSpecialRequestedSignal(SpecialTileType.Bomb));

            Assert.AreEqual(1, m_CreatedCount);
            m_Board.TryGet(m_CreatedPosition, out Tile tile);
            Assert.AreEqual(SpecialTileType.Bomb, tile.Special);
            Assert.AreEqual(TileColor.Red, tile.Color);
            Assert.AreEqual(1, CountSpecials());
        }

        [Test]
        public void RequestIsIgnoredWhileInputIsLocked()
        {
            m_GamePipe.Raise(new InputLockChangedSignal(true));

            m_GamePipe.Raise(new DebugSpecialRequestedSignal(SpecialTileType.Bomb));

            Assert.AreEqual(0, m_CreatedCount);
            Assert.AreEqual(0, CountSpecials());
        }

        private int CountSpecials()
        {
            int count = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    m_Board.TryGet(new GridPosition(x, y), out Tile tile);
                    if (tile.Special != SpecialTileType.None)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void OnSpecialTileCreated(ref SpecialTileCreatedSignal signal)
        {
            m_CreatedCount++;
            m_CreatedPosition = signal.Position;
        }
    }
}
