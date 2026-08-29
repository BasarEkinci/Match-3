using System;
using System.Collections.Generic;
using Match3.Controller;
using Match3.Model.Boosters;
using Match3.Model.Enums;
using Match3.Signals;
using NUnit.Framework;
using Syntac.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class BoosterControllerTests
    {
        private const int Seed = 1;
        private const int Remainder = 10;

        private static readonly TestBoosterSettings Settings = new TestBoosterSettings();

        private GamePipe m_GamePipe;
        private ProjectPipe m_ProjectPipe;
        private BoosterModel m_Model;
        private BoosterController m_Controller;
        private List<BoosterChargeChangedSignal> m_Charges;
        private List<BoosterGrantedSignal> m_Grants;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_Charges = new List<BoosterChargeChangedSignal>();
            m_Grants = new List<BoosterGrantedSignal>();
            m_GamePipe.SubscribeTo<BoosterChargeChangedSignal>(OnChargeChanged);
            m_GamePipe.SubscribeTo<BoosterGrantedSignal>(OnGranted);
            m_Model = new BoosterModel(Settings, new Random(Seed));
            m_Controller = new BoosterController(m_GamePipe, m_ProjectPipe, m_Model);
        }

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
            m_ProjectPipe.Dispose();
        }

        [Test]
        public void ScoreBelowThresholdOnlyReportsCharge()
        {
            RaiseScore(Settings.ScorePerBooster - 1);

            Assert.AreEqual(0, m_Grants.Count);
            Assert.AreEqual(Settings.ScorePerBooster - 1, m_Charges[0].Charge);
            Assert.AreEqual(Settings.ScorePerBooster, m_Charges[0].RequiredCharge);
        }

        [Test]
        public void ThresholdGrantsBoosterAndKeepsRemainder()
        {
            RaiseScore(Settings.ScorePerBooster + Remainder);

            Assert.AreEqual(1, m_Grants.Count);
            Assert.AreEqual(1, m_Model.CountOf(m_Grants[0].Booster));
            Assert.AreEqual(Remainder, m_Charges[0].Charge);
        }

        [Test]
        public void SingleScoreCanGrantMultipleBoosters()
        {
            RaiseScore(Settings.ScorePerBooster * 2);

            Assert.AreEqual(2, m_Grants.Count);
            Assert.AreEqual(Settings.MaxCarried, m_Model.CarriedCount);
        }

        [Test]
        public void FullInventoryStopsChargingAndGranting()
        {
            RaiseScore(Settings.ScorePerBooster * 2);
            RaiseScore(Settings.ScorePerBooster * 5);

            Assert.AreEqual(Settings.MaxCarried, m_Model.CarriedCount);
            Assert.AreEqual(2, m_Grants.Count);
            Assert.AreEqual(Settings.ScorePerBooster, m_Charges[1].Charge);
        }

        [Test]
        public void NewRoundClearsChargeAndInventory()
        {
            RaiseScore(Settings.ScorePerBooster + Remainder);

            m_ProjectPipe.Raise(new RoundStartedSignal(false));

            Assert.AreEqual(0, m_Model.CarriedCount);
            Assert.AreEqual(0, m_Charges[m_Charges.Count - 1].Charge);
        }

        [Test]
        public void DisposedControllerStopsCharging()
        {
            m_Controller.Dispose();

            RaiseScore(Settings.ScorePerBooster);

            Assert.AreEqual(0, m_Charges.Count);
            Assert.AreEqual(0, m_Grants.Count);
        }

        private void RaiseScore(int delta) => m_GamePipe.Raise(new ScoreChangedSignal(delta, delta, 1f));

        private void OnChargeChanged(ref BoosterChargeChangedSignal signal) => m_Charges.Add(signal);

        private void OnGranted(ref BoosterGrantedSignal signal) => m_Grants.Add(signal);
    }
}
