using System.Collections.Generic;
using Match3.Controller;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Signals;
using NUnit.Framework;
using Syntac.MessagePipe.Pipes;

namespace Match3.Tests.EditMode
{
    public sealed class ScreenFlowControllerTests
    {
        private const int Score = 1200;
        private const int Delta = 200;
        private const float Multiplier = 1f;

        private GamePipe m_GamePipe;
        private ProjectPipe m_ProjectPipe;
        private ScreenFlowController m_Controller;
        private FakeSaveRepository m_Save;
        private List<GameScreen> m_Screens;
        private List<bool> m_ResumeFlags;
        private int m_RoundStartedCount;
        private List<int> m_RoundEndScores;

        [SetUp]
        public void SetUp()
        {
            m_GamePipe = new GamePipe();
            m_ProjectPipe = new ProjectPipe();
            m_Save = new FakeSaveRepository();
            m_Screens = new List<GameScreen>();
            m_ResumeFlags = new List<bool>();
            m_RoundEndScores = new List<int>();
            m_RoundStartedCount = 0;
            m_ProjectPipe.SubscribeTo<ScreenChangedSignal>(OnScreenChanged);
            m_ProjectPipe.SubscribeTo<RoundStartedSignal>(OnRoundStarted);
            m_ProjectPipe.SubscribeTo<RoundEndedSignal>(OnRoundEnded);
            m_Controller = new ScreenFlowController(m_GamePipe, m_ProjectPipe, m_Save);
        }

        [TearDown]
        public void TearDown()
        {
            m_Controller.Dispose();
            m_GamePipe.Dispose();
            m_ProjectPipe.Dispose();
        }

        [Test]
        public void PlayingFromMainStartsARound()
        {
            Request(GameScreen.Game);

            Assert.AreEqual(1, m_Screens.Count);
            Assert.AreEqual(GameScreen.Game, m_Screens[0]);
            Assert.AreEqual(1, m_RoundStartedCount);
        }

        [Test]
        public void ResumingFromPauseDoesNotRestartTheRound()
        {
            Request(GameScreen.Game);
            Request(GameScreen.Pause);
            Request(GameScreen.Game);

            Assert.AreEqual(GameScreen.Game, m_Screens[m_Screens.Count - 1]);
            Assert.AreEqual(1, m_RoundStartedCount);
        }

        [Test]
        public void RoundEndCarriesTheCurrentScore()
        {
            Request(GameScreen.Game);
            m_GamePipe.Raise(new ScoreChangedSignal(Score, Delta, Multiplier));

            Request(GameScreen.RoundEnd);

            Assert.AreEqual(1, m_RoundEndScores.Count);
            Assert.AreEqual(Score, m_RoundEndScores[0]);
        }

        [Test]
        public void PlayingAgainFromRoundEndStartsANewRound()
        {
            Request(GameScreen.Game);
            Request(GameScreen.RoundEnd);
            Request(GameScreen.Game);

            Assert.AreEqual(2, m_RoundStartedCount);
        }

        [Test]
        public void RestartFromPauseStartsAFreshRoundInGame()
        {
            Request(GameScreen.Game);
            Request(GameScreen.Pause);

            m_ProjectPipe.Raise(new RoundRestartRequestedSignal());

            Assert.AreEqual(GameScreen.Game, m_Screens[m_Screens.Count - 1]);
            Assert.AreEqual(2, m_RoundStartedCount);
        }

        [Test]
        public void RestartOutsideARoundIsIgnored()
        {
            m_ProjectPipe.Raise(new RoundRestartRequestedSignal());

            Assert.AreEqual(0, m_Screens.Count);
            Assert.AreEqual(0, m_RoundStartedCount);
        }

        [Test]
        public void EndingTheRoundFromPauseShowsTheSummary()
        {
            Request(GameScreen.Game);
            m_GamePipe.Raise(new ScoreChangedSignal(Score, Delta, Multiplier));
            Request(GameScreen.Pause);

            Request(GameScreen.RoundEnd);

            Assert.AreEqual(GameScreen.RoundEnd, m_Screens[m_Screens.Count - 1]);
            Assert.AreEqual(Score, m_RoundEndScores[0]);
        }

        [Test]
        public void FirstRoundResumesWhenASaveExists()
        {
            m_Save.Save(new Board(1, 1), Score);

            Request(GameScreen.Game);

            Assert.IsTrue(m_ResumeFlags[0]);
        }

        [Test]
        public void LaterRoundsNeverResume()
        {
            m_Save.Save(new Board(1, 1), Score);
            Request(GameScreen.Game);

            Request(GameScreen.RoundEnd);
            Request(GameScreen.Game);

            Assert.IsFalse(m_ResumeFlags[1]);
        }

        [Test]
        public void UnreachableScreensAreIgnored()
        {
            Request(GameScreen.Pause);
            Request(GameScreen.RoundEnd);

            Assert.AreEqual(0, m_Screens.Count);
            Assert.AreEqual(0, m_RoundStartedCount);
            Assert.AreEqual(0, m_RoundEndScores.Count);
        }

        [Test]
        public void LeavingPauseToMainEndsNothing()
        {
            Request(GameScreen.Game);
            Request(GameScreen.Pause);
            Request(GameScreen.Main);

            Assert.AreEqual(GameScreen.Main, m_Screens[m_Screens.Count - 1]);
            Assert.AreEqual(0, m_RoundEndScores.Count);
        }

        [Test]
        public void DisposedControllerStopsSwitchingScreens()
        {
            m_Controller.Dispose();

            Request(GameScreen.Game);

            Assert.AreEqual(0, m_Screens.Count);
        }

        private void Request(GameScreen screen)
        {
            m_ProjectPipe.Raise(new ScreenChangeRequestedSignal(screen));
        }

        private void OnScreenChanged(ref ScreenChangedSignal signal) => m_Screens.Add(signal.Screen);

        private void OnRoundStarted(ref RoundStartedSignal signal)
        {
            m_RoundStartedCount++;
            m_ResumeFlags.Add(signal.IsResumed);
        }

        private void OnRoundEnded(ref RoundEndedSignal signal) => m_RoundEndScores.Add(signal.Score);
    }
}
