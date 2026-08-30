using Match3.Model.Enums;
using Match3.Signals;
using Match3.Core.MessagePipe.Pipes;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Match3.View
{
    public sealed class MainScreenView : MonoBehaviour
    {
        [SerializeField] private Button playButton;

        private ProjectPipe m_ProjectPipe;

        [Inject]
        public void Construct(ProjectPipe projectPipe)
        {
            m_ProjectPipe = projectPipe;
            playButton.onClick.AddListener(RequestGameScreen);
            m_ProjectPipe.SubscribeTo<ScreenChangedSignal>(OnScreenChanged);
        }

        private void OnDestroy()
        {
            m_ProjectPipe?.UnsubscribeFrom<ScreenChangedSignal>(OnScreenChanged);
        }

        private void RequestGameScreen()
        {
            m_ProjectPipe.Raise(new ScreenChangeRequestedSignal(GameScreen.Game));
        }

        private void OnScreenChanged(ref ScreenChangedSignal signal)
        {
            gameObject.SetActive(signal.Screen == GameScreen.Main);
        }
    }
}
