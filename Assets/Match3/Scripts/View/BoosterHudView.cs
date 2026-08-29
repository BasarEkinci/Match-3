using System;
using Match3.Model.Enums;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Match3.View
{
    public sealed class BoosterHudView : IDisposable
    {
        private const string RootName = "BoosterHud";
        private const string BarName = "ChargeBar";
        private const string FillName = "ChargeFill";
        private const string LabelName = "Label";
        private const string ButtonFormat = "{0} x{1}";
        private const float BarWidth = 720f;
        private const float BarHeight = 24f;
        private const float BarMargin = 120f;
        private const float ButtonWidth = 280f;
        private const float ButtonHeight = 120f;
        private const float ButtonSpacing = 24f;
        private const float ButtonMargin = 180f;
        private const float ButtonFontSize = 40f;
        private const float Half = 0.5f;
        private const int EmptyCount = 0;

        private static readonly Color BarBackgroundColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
        private static readonly Color BarFillColor = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color ReadyColor = new Color(0.2f, 0.3f, 0.45f, 0.95f);
        private static readonly Color SelectedColor = new Color(0.4f, 0.8f, 1f, 0.95f);
        private static readonly Color EmptyColor = new Color(0.2f, 0.2f, 0.25f, 0.6f);

        private readonly GamePipe m_GamePipe;
        private readonly ProjectPipe m_ProjectPipe;
        private readonly TMP_FontAsset m_Font;
        private readonly GameObject m_Root;
        private readonly Image m_ChargeFill;
        private readonly Image[] m_Backgrounds;
        private readonly TMP_Text[] m_Labels;
        private readonly int[] m_Counts;

        private BoosterType m_SelectedBooster;
        private bool m_HasSelection;
        private bool m_IsDisposed;

        public BoosterHudView(GamePipe gamePipe, ProjectPipe projectPipe, TMP_FontAsset font)
        {
            m_GamePipe = gamePipe;
            m_ProjectPipe = projectPipe;
            m_Font = font;

            int boosterCount = Enum.GetValues(typeof(BoosterType)).Length;
            m_Backgrounds = new Image[boosterCount];
            m_Labels = new TMP_Text[boosterCount];
            m_Counts = new int[boosterCount];

            m_Root = UiFactory.CreateCanvas(RootName);
            m_ChargeFill = CreateChargeBar();
            for (int index = 0; index < boosterCount; index++)
            {
                CreateButton((BoosterType)index, index, boosterCount);
            }

            m_GamePipe.SubscribeTo<BoosterChargeChangedSignal>(OnChargeChanged);
            m_GamePipe.SubscribeTo<BoosterGrantedSignal>(OnBoosterGranted);
            m_GamePipe.SubscribeTo<BoosterAppliedSignal>(OnBoosterApplied);
            m_GamePipe.SubscribeTo<BoosterSelectionChangedSignal>(OnSelectionChanged);
            m_ProjectPipe.SubscribeTo<RoundStartedSignal>(OnRoundStarted);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_GamePipe.UnsubscribeFrom<BoosterChargeChangedSignal>(OnChargeChanged);
            m_GamePipe.UnsubscribeFrom<BoosterGrantedSignal>(OnBoosterGranted);
            m_GamePipe.UnsubscribeFrom<BoosterAppliedSignal>(OnBoosterApplied);
            m_GamePipe.UnsubscribeFrom<BoosterSelectionChangedSignal>(OnSelectionChanged);
            m_ProjectPipe.UnsubscribeFrom<RoundStartedSignal>(OnRoundStarted);
            if (m_Root != null)
            {
                UnityEngine.Object.Destroy(m_Root);
            }
        }

        private Image CreateChargeBar()
        {
            Image background = CreateImage(
                BarName,
                new Vector2(BarWidth, BarHeight),
                new Vector2(0f, BarMargin),
                BarBackgroundColor);

            Image fill = CreateImage(FillName, new Vector2(BarWidth, BarHeight), Vector2.zero, BarFillColor);
            fill.rectTransform.SetParent(background.rectTransform, false);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0f;
            return fill;
        }

        private void CreateButton(BoosterType booster, int index, int boosterCount)
        {
            float offsetX = (index - ((boosterCount - 1) * Half)) * (ButtonWidth + ButtonSpacing);
            Image background = CreateImage(
                booster.ToString(),
                new Vector2(ButtonWidth, ButtonHeight),
                new Vector2(offsetX, ButtonMargin),
                EmptyColor);

            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(() => RequestSelection(booster));

            m_Backgrounds[index] = background;
            m_Labels[index] = CreateLabel(background.rectTransform);
            Refresh(index);
        }

        private Image CreateImage(string name, Vector2 size, Vector2 offset, Color color)
        {
            RectTransform rect = UiFactory.CreateRect(name, m_Root.transform);
            rect.anchorMin = new Vector2(Half, 0f);
            rect.anchorMax = new Vector2(Half, 0f);
            rect.pivot = new Vector2(Half, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private TMP_Text CreateLabel(RectTransform parent)
        {
            return UiFactory.CreateText(
                UiFactory.Stretch(UiFactory.CreateRect(LabelName, parent)),
                string.Empty,
                m_Font,
                ButtonFontSize,
                Color.white);
        }

        private void RequestSelection(BoosterType booster)
        {
            if (m_Counts[(int)booster] == EmptyCount)
            {
                return;
            }

            m_GamePipe.Raise(new BoosterSelectionRequestedSignal(booster));
        }

        private void OnRoundStarted(ref RoundStartedSignal signal)
        {
            Array.Clear(m_Counts, 0, m_Counts.Length);
            RefreshAll();
        }

        private void OnChargeChanged(ref BoosterChargeChangedSignal signal)
        {
            m_ChargeFill.fillAmount = (float)signal.Charge / signal.RequiredCharge;
        }

        private void OnBoosterGranted(ref BoosterGrantedSignal signal)
        {
            m_Counts[(int)signal.Booster]++;
            Refresh((int)signal.Booster);
        }

        private void OnBoosterApplied(ref BoosterAppliedSignal signal)
        {
            m_Counts[(int)signal.Booster]--;
            Refresh((int)signal.Booster);
        }

        private void OnSelectionChanged(ref BoosterSelectionChangedSignal signal)
        {
            m_SelectedBooster = signal.Booster;
            m_HasSelection = signal.IsActive;
            RefreshAll();
        }

        private void RefreshAll()
        {
            for (int index = 0; index < m_Backgrounds.Length; index++)
            {
                Refresh(index);
            }
        }

        private void Refresh(int index)
        {
            m_Labels[index].text = string.Format(ButtonFormat, (BoosterType)index, m_Counts[index]);
            m_Backgrounds[index].color = Tint(index);
        }

        private Color Tint(int index)
        {
            if (m_Counts[index] == EmptyCount)
            {
                return EmptyColor;
            }

            return m_HasSelection && (int)m_SelectedBooster == index ? SelectedColor : ReadyColor;
        }
    }
}
