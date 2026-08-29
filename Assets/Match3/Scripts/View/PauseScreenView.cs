using System;
using Match3.Model.Enums;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;
using TMPro;
using UnityEngine;

namespace Match3.View
{
    public sealed class PauseScreenView : IDisposable
    {
        private const string RootName = "PauseScreen";
        private const string ButtonRootName = "PauseButton";
        private const string TitleText = "PAUSED";
        private const string ResumeText = "RESUME";
        private const string RestartText = "RESTART";
        private const string EndRoundText = "END ROUND";
        private const string PauseText = "II";
        private const int SortingOrder = 90;
        private const int ButtonSortingOrder = 80;
        private const float ButtonSize = 110f;
        private const float ButtonMargin = 48f;
        private const float ButtonFontSize = 48f;
        private const float PausedTimeScale = 0f;
        private const float RunningTimeScale = 1f;

        private static readonly Color ButtonColor = new Color(0.2f, 0.3f, 0.45f, 0.9f);

        private readonly ProjectPipe m_ProjectPipe;
        private readonly MenuScreen m_Menu;
        private readonly GameObject m_ButtonRoot;

        private bool m_IsDisposed;

        public PauseScreenView(ProjectPipe projectPipe, TMP_FontAsset font)
        {
            m_ProjectPipe = projectPipe;
            m_Menu = new MenuScreen(RootName, font, SortingOrder, TitleText);
            m_Menu.AddButton(ResumeText, RequestResume);
            m_Menu.AddButton(RestartText, RequestRestart);
            m_Menu.AddButton(EndRoundText, RequestRoundEnd);
            m_Menu.IsVisible = false;

            m_ButtonRoot = CreatePauseButton(font);
            m_ButtonRoot.SetActive(false);

            m_ProjectPipe.SubscribeTo<ScreenChangedSignal>(OnScreenChanged);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            Time.timeScale = RunningTimeScale;
            m_ProjectPipe.UnsubscribeFrom<ScreenChangedSignal>(OnScreenChanged);
            m_Menu.Dispose();
            if (m_ButtonRoot != null)
            {
                UnityEngine.Object.Destroy(m_ButtonRoot);
            }
        }

        private GameObject CreatePauseButton(TMP_FontAsset font)
        {
            GameObject root = UiFactory.CreateCanvas(ButtonRootName);
            root.GetComponent<Canvas>().sortingOrder = ButtonSortingOrder;

            RectTransform rect = UiFactory.CreateRect(ButtonRootName, root.transform);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(ButtonSize, ButtonSize);
            rect.anchoredPosition = new Vector2(ButtonMargin, -ButtonMargin);

            UiFactory.CreateButton(rect, PauseText, font, ButtonFontSize, ButtonColor, Color.white, RequestPause);
            return root;
        }

        private void RequestPause() => Request(GameScreen.Pause);

        private void RequestResume() => Request(GameScreen.Game);

        private void RequestRoundEnd() => Request(GameScreen.RoundEnd);

        private void RequestRestart()
        {
            m_ProjectPipe.Raise(new RoundRestartRequestedSignal());
        }

        private void Request(GameScreen screen)
        {
            m_ProjectPipe.Raise(new ScreenChangeRequestedSignal(screen));
        }

        private void OnScreenChanged(ref ScreenChangedSignal signal)
        {
            bool isPaused = signal.Screen == GameScreen.Pause;
            m_Menu.IsVisible = isPaused;
            m_ButtonRoot.SetActive(signal.Screen == GameScreen.Game);
            Time.timeScale = isPaused ? PausedTimeScale : RunningTimeScale;
        }
    }
}
