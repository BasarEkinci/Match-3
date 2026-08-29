using System;
using Match3.Model.Enums;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;
using TMPro;
using UnityEngine;

namespace Match3.View
{
    public sealed class MainScreenView : IDisposable
    {
        private const string RootName = "MainScreen";
        private const string PlayText = "PLAY";
        private const int SortingOrder = 100;

        private readonly ProjectPipe m_ProjectPipe;
        private readonly MenuScreen m_Menu;

        private bool m_IsDisposed;

        public MainScreenView(ProjectPipe projectPipe, TMP_FontAsset font)
        {
            m_ProjectPipe = projectPipe;
            m_Menu = new MenuScreen(RootName, font, SortingOrder, Application.productName);
            m_Menu.AddButton(PlayText, RequestGameScreen);

            m_ProjectPipe.SubscribeTo<ScreenChangedSignal>(OnScreenChanged);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_ProjectPipe.UnsubscribeFrom<ScreenChangedSignal>(OnScreenChanged);
            m_Menu.Dispose();
        }

        private void RequestGameScreen()
        {
            m_ProjectPipe.Raise(new ScreenChangeRequestedSignal(GameScreen.Game));
        }

        private void OnScreenChanged(ref ScreenChangedSignal signal)
        {
            m_Menu.IsVisible = signal.Screen == GameScreen.Main;
        }
    }
}
