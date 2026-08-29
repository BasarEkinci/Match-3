using System.Collections.Generic;
using Match3.Controller;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Signals;
using NUnit.Framework;
using Syntac.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class BoosterSelectionTests
    {
        private static readonly GridPosition Target = new GridPosition(2, 3);

        private GamePipe m_GamePipe;
        private InputController m_Controller;
        private List<BoosterUseRequestedSignal> m_Uses;
        private List<BoosterSelectionChangedSignal> m_Selections;
        private List<SwapRequestedSignal> m_Swaps;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_Uses = new List<BoosterUseRequestedSignal>();
            m_Selections = new List<BoosterSelectionChangedSignal>();
            m_Swaps = new List<SwapRequestedSignal>();
            m_GamePipe.SubscribeTo<BoosterUseRequestedSignal>(OnBoosterUseRequested);
            m_GamePipe.SubscribeTo<BoosterSelectionChangedSignal>(OnSelectionChanged);
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
        public void TapWithoutSelectionRequestsNothing()
        {
            Tap(Target);

            Assert.AreEqual(0, m_Uses.Count);
        }

        [Test]
        public void SelectedBoosterIsAppliedToTappedCell()
        {
            Select(BoosterType.Hammer);
            Tap(Target);

            Assert.AreEqual(1, m_Uses.Count);
            Assert.AreEqual(BoosterType.Hammer, m_Uses[0].Booster);
            Assert.AreEqual(Target, m_Uses[0].Target);
            Assert.IsFalse(m_Selections[m_Selections.Count - 1].IsActive);
        }

        [Test]
        public void SelectionSurvivesOnlyOneUse()
        {
            Select(BoosterType.Hammer);
            Tap(Target);
            Tap(Target);

            Assert.AreEqual(1, m_Uses.Count);
        }

        [Test]
        public void SelectingSameBoosterAgainCancelsSelection()
        {
            Select(BoosterType.Hammer);
            Select(BoosterType.Hammer);
            Tap(Target);

            Assert.AreEqual(0, m_Uses.Count);
            Assert.IsFalse(m_Selections[m_Selections.Count - 1].IsActive);
        }

        [Test]
        public void SelectingAnotherBoosterKeepsSelectionActive()
        {
            Select(BoosterType.Hammer);
            Select(BoosterType.ColorPicker);
            Tap(Target);

            Assert.AreEqual(1, m_Uses.Count);
            Assert.AreEqual(BoosterType.ColorPicker, m_Uses[0].Booster);
        }

        [Test]
        public void SelectionModeBlocksSwaps()
        {
            Select(BoosterType.Hammer);

            m_GamePipe.Raise(new TileDragSignal(Target, GridDirection.Up));

            Assert.AreEqual(0, m_Swaps.Count);
        }

        [Test]
        public void ShuffleIsAppliedWithoutTargeting()
        {
            Select(BoosterType.Shuffle);

            Assert.AreEqual(1, m_Uses.Count);
            Assert.AreEqual(BoosterType.Shuffle, m_Uses[0].Booster);
            Assert.IsFalse(m_Selections[m_Selections.Count - 1].IsActive);
        }

        [Test]
        public void LockedInputCancelsSelectionAndRefusesNewOnes()
        {
            Select(BoosterType.Hammer);

            m_GamePipe.Raise(new InputLockChangedSignal(true));
            Select(BoosterType.Hammer);
            Tap(Target);

            Assert.AreEqual(0, m_Uses.Count);
        }

        private void Select(BoosterType booster)
        {
            m_GamePipe.Raise(new BoosterSelectionRequestedSignal(booster));
        }

        private void Tap(GridPosition origin)
        {
            m_GamePipe.Raise(new TileTapSignal(origin));
        }

        private void OnBoosterUseRequested(ref BoosterUseRequestedSignal signal) => m_Uses.Add(signal);

        private void OnSelectionChanged(ref BoosterSelectionChangedSignal signal) => m_Selections.Add(signal);

        private void OnSwapRequested(ref SwapRequestedSignal signal) => m_Swaps.Add(signal);
    }
}
