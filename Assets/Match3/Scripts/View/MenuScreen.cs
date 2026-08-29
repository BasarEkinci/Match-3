using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Match3.View
{
    public sealed class MenuScreen : IDisposable
    {
        private const string BackgroundName = "Background";
        private const string TitleName = "Title";
        private const string MessageName = "Message";
        private const string ButtonName = "Button";
        private const float TitleFontSize = 96f;
        private const float MessageFontSize = 56f;
        private const float ButtonFontSize = 52f;
        private const float LineHeight = 160f;
        private const float TitleOffset = 480f;
        private const float MessageOffset = 300f;
        private const float FirstButtonOffset = 60f;
        private const float TextWidth = 900f;
        private const float ButtonWidth = 560f;
        private const float ButtonHeight = 130f;
        private const float ButtonSpacing = 30f;
        private const float Half = 0.5f;

        private static readonly Color BackgroundColor = new Color(0.06f, 0.07f, 0.11f, 0.96f);
        private static readonly Color ButtonColor = new Color(0.4f, 0.8f, 1f, 0.95f);
        private static readonly Color ButtonTextColor = new Color(0.05f, 0.07f, 0.1f, 1f);

        private readonly TMP_FontAsset m_Font;
        private readonly GameObject m_Root;
        private readonly TMP_Text m_Message;

        private int m_ButtonCount;

        public MenuScreen(string name, TMP_FontAsset font, int sortingOrder, string title)
        {
            m_Font = font;
            m_Root = UiFactory.CreateCanvas(name);
            m_Root.GetComponent<Canvas>().sortingOrder = sortingOrder;

            UiFactory.Stretch(UiFactory.CreateRect(BackgroundName, m_Root.transform))
                .gameObject.AddComponent<Image>().color = BackgroundColor;

            UiFactory.CreateText(
                CreateCentredRect(TitleName, TitleOffset, new Vector2(TextWidth, LineHeight)),
                title,
                m_Font,
                TitleFontSize,
                Color.white);

            m_Message = UiFactory.CreateText(
                CreateCentredRect(MessageName, MessageOffset, new Vector2(TextWidth, LineHeight)),
                string.Empty,
                m_Font,
                MessageFontSize,
                Color.white);
        }

        public bool IsVisible
        {
            set => m_Root.SetActive(value);
        }

        public string Message
        {
            set => m_Message.text = value;
        }

        public void AddButton(string text, Action onClick)
        {
            float offset = FirstButtonOffset - (m_ButtonCount * (ButtonHeight + ButtonSpacing));
            m_ButtonCount++;

            UiFactory.CreateButton(
                CreateCentredRect(ButtonName + text, offset, new Vector2(ButtonWidth, ButtonHeight)),
                text,
                m_Font,
                ButtonFontSize,
                ButtonColor,
                ButtonTextColor,
                onClick);
        }

        public void Dispose()
        {
            if (m_Root != null)
            {
                UnityEngine.Object.Destroy(m_Root);
            }
        }

        private RectTransform CreateCentredRect(string name, float offsetY, Vector2 size)
        {
            RectTransform rect = UiFactory.CreateRect(name, m_Root.transform);
            rect.anchorMin = new Vector2(Half, Half);
            rect.anchorMax = new Vector2(Half, Half);
            rect.pivot = new Vector2(Half, Half);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(0f, offsetY);
            return rect;
        }
    }
}
