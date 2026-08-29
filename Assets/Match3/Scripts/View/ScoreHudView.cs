using System;
using LitMotion;
using LitMotion.Extensions;
using Match3.Model.Settings;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;
using TMPro;
using UnityEngine;

namespace Match3.View
{
    public sealed class ScoreHudView : IDisposable
    {
        private const string RootName = "Hud";
        private const string ScoreLabelName = "Score";
        private const string MultiplierLabelName = "Multiplier";
        private const string MultiplierFormat = "x{0:0.0}";
        private const float Margin = 48f;
        private const float ScoreFontSize = 96f;
        private const float LabelFontSize = 48f;
        private const float LabelWidth = 520f;
        private const float LabelHeight = 120f;
        private const float CounterDuration = 0.35f;

        private readonly GamePipe m_GamePipe;
        private readonly GameObject m_Root;
        private readonly TMP_Text m_ScoreText;
        private readonly TMP_Text m_MultiplierText;
        private readonly TMP_FontAsset m_Font;

        private MotionHandle m_CounterHandle;
        private int m_DisplayedScore;
        private bool m_IsDisposed;

        public ScoreHudView(GamePipe gamePipe, IScoreSettings settings, TMP_FontAsset font)
        {
            m_GamePipe = gamePipe;
            m_Font = font;
            m_Root = UiFactory.CreateCanvas(RootName);
            m_ScoreText = CreateLabel(ScoreLabelName, new Vector2(0.5f, 1f), new Vector2(0f, -Margin), ScoreFontSize, TextAlignmentOptions.Top);
            m_MultiplierText = CreateLabel(MultiplierLabelName, new Vector2(1f, 1f), new Vector2(-Margin, -Margin), LabelFontSize, TextAlignmentOptions.TopRight);
            m_ScoreText.text = m_DisplayedScore.ToString();
            m_MultiplierText.text = string.Format(MultiplierFormat, settings.BaseMultiplier);

            m_GamePipe.SubscribeTo<ScoreChangedSignal>(OnScoreChanged);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_CounterHandle.TryCancel();
            m_GamePipe.UnsubscribeFrom<ScoreChangedSignal>(OnScoreChanged);
            if (m_Root != null)
            {
                UnityEngine.Object.Destroy(m_Root);
            }
        }

        private TMP_Text CreateLabel(string name, Vector2 anchor, Vector2 offset, float fontSize, TextAlignmentOptions alignment)
        {
            RectTransform rect = UiFactory.CreateRect(name, m_Root.transform);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = new Vector2(LabelWidth, LabelHeight);
            rect.anchoredPosition = offset;

            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.raycastTarget = false;
            if (m_Font != null)
            {
                text.font = m_Font;
            }

            return text;
        }

        private void OnScoreChanged(ref ScoreChangedSignal signal)
        {
            m_MultiplierText.text = string.Format(MultiplierFormat, signal.Multiplier);
            CountTo(signal.Total);
        }

        private void CountTo(int total)
        {
            m_CounterHandle.TryCancel();
            m_CounterHandle = LMotion.Create(m_DisplayedScore, total, CounterDuration)
                .BindToText(m_ScoreText);
            m_DisplayedScore = total;
        }
    }
}
