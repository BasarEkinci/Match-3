using Match3.Model.Settings;
using UnityEngine;

namespace Match3.Data
{
    [CreateAssetMenu(fileName = AssetName, menuName = MenuName)]
    public sealed class HintSettings : ScriptableObject, IHintSettings
    {
        private const string AssetName = "HintSettings";
        private const string MenuName = "Match3/Hint Settings";

        private const float MinDelay = 0.5f;
        private const float MaxDelay = 30f;
        private const float DefaultIdleSeconds = 5f;
        private const float DefaultRepeatSeconds = 3f;
        private const float MinStrength = 0.05f;
        private const float MaxStrength = 1f;
        private const float DefaultStrength = 0.35f;
        private const float MinDuration = 0.1f;
        private const float MaxDuration = 2f;
        private const float DefaultDuration = 0.3f;
        private const int MinFrequency = 1;
        private const int MaxFrequency = 20;
        private const int DefaultFrequency = 6;

        [SerializeField, Range(MinDelay, MaxDelay)]
        private float idleSeconds = DefaultIdleSeconds;

        [SerializeField, Range(MinDelay, MaxDelay)]
        private float repeatSeconds = DefaultRepeatSeconds;

        [SerializeField, Range(MinStrength, MaxStrength)]
        private float highlightStrength = DefaultStrength;

        [SerializeField, Range(MinDuration, MaxDuration)]
        private float highlightDuration = DefaultDuration;

        [SerializeField, Range(MinFrequency, MaxFrequency)]
        private int highlightFrequency = DefaultFrequency;

        public float IdleSeconds => idleSeconds;

        public float RepeatSeconds => repeatSeconds;

        public float HighlightStrength => highlightStrength;

        public float HighlightDuration => highlightDuration;

        public int HighlightFrequency => highlightFrequency;
    }
}
