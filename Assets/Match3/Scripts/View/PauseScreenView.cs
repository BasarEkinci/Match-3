using Match3.Model.Enums;
using Match3.Signals;
using Match3.Core.MessagePipe.Pipes;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Match3.View
{
    public sealed class PauseScreenView : MonoBehaviour
    {
        private const float PausedTimeScale = 0f;
        private const float RunningTimeScale = 1f;

        [SerializeField] private GameObject menuRoot;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button mainMenuButton;

        private ProjectPipe m_ProjectPipe;

        [Inject]
        public void Construct(ProjectPipe projectPipe)
        {
            m_ProjectPipe = projectPipe;

            pauseButton.onClick.AddListener(RequestPause);
            resumeButton.onClick.AddListener(RequestResume);
            resetButton.onClick.AddListener(RequestReset);
            mainMenuButton.onClick.AddListener(RequestMainScreen);

            menuRoot.SetActive(false);
            pauseButton.gameObject.SetActive(false);

            m_ProjectPipe.SubscribeTo<ScreenChangedSignal>(OnScreenChanged);
        }

        private void OnDestroy()
        {
            Time.timeScale = RunningTimeScale;
            m_ProjectPipe?.UnsubscribeFrom<ScreenChangedSignal>(OnScreenChanged);
        }

        private void RequestPause() => Request(GameScreen.Pause);

        private void RequestResume() => Request(GameScreen.Game);

        private void RequestMainScreen() => Request(GameScreen.Main);

        private void RequestReset()
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
            menuRoot.SetActive(isPaused);
            pauseButton.gameObject.SetActive(signal.Screen == GameScreen.Game);
            Time.timeScale = isPaused ? PausedTimeScale : RunningTimeScale;
        }
    }
}
