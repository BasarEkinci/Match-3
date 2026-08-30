using LitMotion;
using LitMotion.Extensions;
using Match3.Model.Settings;
using Match3.Signals;
using Match3.Core.MessagePipe.Pipes;
using TMPro;
using UnityEngine;
using VContainer;

namespace Match3.View
{
    public sealed class ScoreHudView : MonoBehaviour
    {
        private const string MultiplierFormat = "x{0:0.0}";
        private const float CounterDuration = 0.35f;

        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text multiplierText;

        private GamePipe m_GamePipe;
        private MotionHandle m_CounterHandle;
        private int m_DisplayedScore;

        [Inject]
        public void Construct(GamePipe gamePipe, IScoreSettings settings)
        {
            m_GamePipe = gamePipe;
            scoreText.text = m_DisplayedScore.ToString();
            multiplierText.text = string.Format(MultiplierFormat, settings.BaseMultiplier);

            m_GamePipe.SubscribeTo<ScoreChangedSignal>(OnScoreChanged);
        }

        private void OnDestroy()
        {
            m_CounterHandle.TryCancel();
            m_GamePipe?.UnsubscribeFrom<ScoreChangedSignal>(OnScoreChanged);
        }

        private void OnScoreChanged(ref ScoreChangedSignal signal)
        {
            multiplierText.text = string.Format(MultiplierFormat, signal.Multiplier);
            CountTo(signal.Total);
        }

        private void CountTo(int total)
        {
            m_CounterHandle.TryCancel();
            m_CounterHandle = LMotion.Create(m_DisplayedScore, total, CounterDuration)
                .BindToText(scoreText);
            m_DisplayedScore = total;
        }
    }
}
