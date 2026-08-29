using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Match3.View
{
    public static class UiFactory
    {
        private const string EventSystemName = "EventSystem";
        private const string LabelName = "Label";
        private const float ReferenceWidth = 1080f;
        private const float ReferenceHeight = 1920f;
        private const float MatchWidthOverHeight = 0.5f;

        public static GameObject CreateCanvas(string name)
        {
            EnsureEventSystem();

            GameObject root = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = MatchWidthOverHeight;
            return root;
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        public static RectTransform Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static Button CreateButton(
            RectTransform rect,
            string text,
            TMP_FontAsset font,
            float fontSize,
            Color backgroundColor,
            Color textColor,
            Action onClick)
        {
            Image background = rect.gameObject.AddComponent<Image>();
            background.color = backgroundColor;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(() => onClick());

            CreateText(Stretch(CreateRect(LabelName, rect)), text, font, fontSize, textColor);
            return button;
        }

        public static TMP_Text CreateText(RectTransform rect, string content, TMP_FontAsset font, float fontSize, Color color)
        {
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            if (font != null)
            {
                text.font = font;
            }

            return text;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            new GameObject(EventSystemName, typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }
}
