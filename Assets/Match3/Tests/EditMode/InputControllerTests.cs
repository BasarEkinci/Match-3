using System.Collections.Generic;
using Match3.Controller;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Signals;
using NUnit.Framework;
using Match3.Core.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class InputControllerTests
    {
        private GamePipe m_GamePipe;
        private InputController m_Controller;
        private List<SwapRequestedSignal> m_Requests;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_Requests = new List<SwapRequestedSignal>();
            m_GamePipe.SubscribeTo<SwapRequestedSignal>(OnSwapRequested);
            m_Controller = new InputController(m_GamePipe);
        }

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
        }

        [Test]
        public void DragUpRequestsSwapWithUpperNeighbour()
        {
            Drag(new GridPosition(2, 3), GridDirection.Up);

            AssertSingleRequest(new GridPosition(2, 3), new GridPosition(2, 4));
        }

        [Test]
        public void DragDownRequestsSwapWithLowerNeighbour()
        {
            Drag(new GridPosition(2, 3), GridDirection.Down);

            AssertSingleRequest(new GridPosition(2, 3), new GridPosition(2, 2));
        }

        [Test]
        public void DragLeftRequestsSwapWithLeftNeighbour()
        {
            Drag(new GridPosition(2, 3), GridDirection.Left);

            AssertSingleRequest(new GridPosition(2, 3), new GridPosition(1, 3));
        }

        [Test]
        public void DragRightRequestsSwapWithRightNeighbour()
        {
            Drag(new GridPosition(2, 3), GridDirection.Right);

            AssertSingleRequest(new GridPosition(2, 3), new GridPosition(3, 3));
        }

        [Test]
        public void LockedInputPublishesNothing()
        {
            m_GamePipe.Raise(new InputLockChangedSignal(true));

            Drag(new GridPosition(0, 0), GridDirection.Right);
            Drag(new GridPosition(1, 1), GridDirection.Up);

            Assert.AreEqual(0, m_Requests.Count);
        }

        [Test]
        public void UnlockedInputResumesPublishing()
        {
            m_GamePipe.Raise(new InputLockChangedSignal(true));
            Drag(new GridPosition(0, 0), GridDirection.Right);

            m_GamePipe.Raise(new InputLockChangedSignal(false));
            Drag(new GridPosition(0, 0), GridDirection.Right);

            AssertSingleRequest(new GridPosition(0, 0), new GridPosition(1, 0));
        }

        [Test]
        public void DisposedControllerPublishesNothing()
        {
            m_Controller.Dispose();

            Drag(new GridPosition(0, 0), GridDirection.Right);

            Assert.AreEqual(0, m_Requests.Count);
        }

        private void Drag(GridPosition origin, GridDirection direction)
        {
            m_GamePipe.Raise(new TileDragSignal(origin, direction));
        }

        private void AssertSingleRequest(GridPosition from, GridPosition to)
        {
            Assert.AreEqual(1, m_Requests.Count);
            Assert.AreEqual(from, m_Requests[0].From);
            Assert.AreEqual(to, m_Requests[0].To);
        }

        private void OnSwapRequested(ref SwapRequestedSignal signal) => m_Requests.Add(signal);
    }
}
