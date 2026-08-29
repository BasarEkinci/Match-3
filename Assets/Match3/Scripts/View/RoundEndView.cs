using System;
using Match3.Model.Enums;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;
using TMPro;

namespace Match3.View
{
    public sealed class RoundEndView : IDisposable
    {
        private const string RootName = "RoundEndScreen";
        private const string TitleText = "ROUND OVER";
        private const string ScoreFormat = "SCORE {0}";
        private const string PlayAgainText = "PLAY AGAIN";
        private const string MainMenuText = "MAIN MENU";
        private const int SortingOrder = 95;

        private readonly ProjectPipe m_ProjectPipe;
        private readonly MenuScreen m_Menu;

        private bool m_IsDisposed;

        public RoundEndView(ProjectPipe projectPipe, TMP_FontAsset font)
        {
            m_ProjectPipe = projectPipe;
            m_Menu = new MenuScreen(RootName, font, SortingOrder, TitleText);
            m_Menu.AddButton(PlayAgainText, RequestGameScreen);
            m_Menu.AddButton(MainMenuText, RequestMainScreen);
            m_Menu.IsVisible = false;

            m_ProjectPipe.SubscribeTo<ScreenChangedSignal>(OnScreenChanged);
            m_ProjectPipe.SubscribeTo<RoundEndedSignal>(OnRoundEnded);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_ProjectPipe.UnsubscribeFrom<ScreenChangedSignal>(OnScreenChanged);
            m_ProjectPipe.UnsubscribeFrom<RoundEndedSignal>(OnRoundEnded);
            m_Menu.Dispose();
        }

        private void RequestGameScreen()
        {
            m_ProjectPipe.Raise(new ScreenChangeRequestedSignal(GameScreen.Game));
        }

        private void RequestMainScreen()
        {
            m_ProjectPipe.Raise(new ScreenChangeRequestedSignal(GameScreen.Main));
        }

        private void OnRoundEnded(ref RoundEndedSignal signal)
        {
            m_Menu.Message = string.Format(ScoreFormat, signal.Score);
        }

        private void OnScreenChanged(ref ScreenChangedSignal signal)
        {
            m_Menu.IsVisible = signal.Screen == GameScreen.RoundEnd;
        }
    }
}
