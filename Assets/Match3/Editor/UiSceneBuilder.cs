using Match3.View;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Match3.Editor
{
    public static class UiSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string FontPath = "Assets/Match3/Art/Fonts/Pixel Game SDF.asset";
        private const string MenuPath = "Match3/Build UI Canvas";
        private const string CanvasName = "UI";
        private const string EventSystemName = "EventSystem";
        private const string BackgroundName = "Background";
        private const string TitleName = "Title";
        private const string LabelName = "Label";
        private const string HudName = "Hud";
        private const string ScoreName = "Score";
        private const string MultiplierName = "Multiplier";
        private const string MainScreenName = "MainScreen";
        private const string PlayButtonName = "PlayButton";
        private const string PauseName = "Pause";
        private const string PauseMenuName = "PauseMenu";
        private const string PauseButtonName = "PauseButton";
        private const string ResumeButtonName = "ResumeButton";
        private const string ResetButtonName = "ResetButton";
        private const string MainMenuButtonName = "MainMenuButton";

        private const string ScoreTextField = "scoreText";
        private const string MultiplierTextField = "multiplierText";
        private const string PlayButtonField = "playButton";
        private const string MenuRootField = "menuRoot";
        private const string PauseButtonField = "pauseButton";
        private const string ResumeButtonField = "resumeButton";
        private const string ResetButtonField = "resetButton";
        private const string MainMenuButtonField = "mainMenuButton";

        private const string PauseTitleText = "PAUSED";
        private const string PlayText = "PLAY";
        private const string ResumeText = "RESUME";
        private const string ResetText = "RESET";
        private const string MainMenuText = "MAIN MENU";
        private const string PauseGlyphText = "II";
        private const string BuiltMessage = "UI canvas built into ";

        private const float ReferenceWidth = 1080f;
        private const float ReferenceHeight = 1920f;
        private const float MatchWidthOverHeight = 0.5f;
        private const float Half = 0.5f;
        private const float Margin = 48f;
        private const float TitleFontSize = 96f;
        private const float ButtonFontSize = 52f;
        private const float ScoreFontSize = 96f;
        private const float MultiplierFontSize = 48f;
        private const float PauseGlyphFontSize = 48f;
        private const float LineHeight = 160f;
        private const float TitleOffset = 480f;
        private const float FirstButtonOffset = 60f;
        private const float TextWidth = 900f;
        private const float MenuButtonWidth = 560f;
        private const float MenuButtonHeight = 130f;
        private const float MenuButtonSpacing = 30f;
        private const float LabelWidth = 520f;
        private const float LabelHeight = 120f;
        private const float PauseButtonSize = 110f;

        private static readonly Vector2 TopCenter = new Vector2(Half, 1f);
        private static readonly Vector2 TopRight = new Vector2(1f, 1f);
        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 Center = new Vector2(Half, Half);

        private static readonly Color OverlayColor = new Color(0.06f, 0.07f, 0.11f, 0.96f);
        private static readonly Color MenuButtonColor = new Color(0.4f, 0.8f, 1f, 0.95f);
        private static readonly Color MenuButtonTextColor = new Color(0.05f, 0.07f, 0.1f, 1f);
        private static readonly Color PauseButtonColor = new Color(0.2f, 0.3f, 0.45f, 0.9f);

        private static TMP_FontAsset s_Font;

        [MenuItem(MenuPath)]
        public static void Build()
        {
            Scene scene = SceneManager.GetActiveScene().path == ScenePath
                ? SceneManager.GetActiveScene()
                : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject existing = FindRoot(scene, CanvasName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            s_Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            EnsureEventSystem(scene);
            Transform canvas = CreateCanvas().transform;
            BuildScoreHud(canvas);
            BuildMainScreen(canvas);
            BuildPause(canvas);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log(BuiltMessage + ScenePath);
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                {
                    return root;
                }
            }

            return null;
        }

        private static void EnsureEventSystem(Scene scene)
        {
            if (FindRoot(scene, EventSystemName) != null)
            {
                return;
            }

            new GameObject(EventSystemName, typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static GameObject CreateCanvas()
        {
            GameObject root = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = MatchWidthOverHeight;
            return root;
        }

        private static void BuildScoreHud(Transform canvas)
        {
            RectTransform root = CreateStretched(HudName, canvas);
            ScoreHudView view = root.gameObject.AddComponent<ScoreHudView>();

            TMP_Text score = CreateText(
                CreateAnchored(ScoreName, root, TopCenter, new Vector2(0f, -Margin), new Vector2(LabelWidth, LabelHeight)),
                string.Empty,
                ScoreFontSize,
                TextAlignmentOptions.Top,
                Color.white);

            TMP_Text multiplier = CreateText(
                CreateAnchored(MultiplierName, root, TopRight, new Vector2(-Margin, -Margin), new Vector2(LabelWidth, LabelHeight)),
                string.Empty,
                MultiplierFontSize,
                TextAlignmentOptions.TopRight,
                Color.white);

            SerializedObject serialized = new SerializedObject(view);
            SetReference(serialized, ScoreTextField, score);
            SetReference(serialized, MultiplierTextField, multiplier);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildMainScreen(Transform canvas)
        {
            RectTransform root = CreateOverlayScreen(MainScreenName, canvas, Application.productName);
            MainScreenView view = root.gameObject.AddComponent<MainScreenView>();
            Button play = CreateMenuButton(root, PlayButtonName, PlayText, 0);

            SerializedObject serialized = new SerializedObject(view);
            SetReference(serialized, PlayButtonField, play);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildPause(Transform canvas)
        {
            RectTransform root = CreateStretched(PauseName, canvas);
            PauseScreenView view = root.gameObject.AddComponent<PauseScreenView>();

            RectTransform menu = CreateOverlayScreen(PauseMenuName, root, PauseTitleText);
            Button resume = CreateMenuButton(menu, ResumeButtonName, ResumeText, 0);
            Button reset = CreateMenuButton(menu, ResetButtonName, ResetText, 1);
            Button mainMenu = CreateMenuButton(menu, MainMenuButtonName, MainMenuText, 2);
            menu.gameObject.SetActive(false);

            RectTransform pauseRect = CreateAnchored(
                PauseButtonName,
                root,
                TopLeft,
                new Vector2(Margin, -Margin),
                new Vector2(PauseButtonSize, PauseButtonSize));
            Button pause = CreateButton(pauseRect, PauseButtonColor);
            CreateText(CreateStretched(LabelName, pauseRect), PauseGlyphText, PauseGlyphFontSize, TextAlignmentOptions.Center, Color.white);
            pauseRect.gameObject.SetActive(false);

            SerializedObject serialized = new SerializedObject(view);
            SetReference(serialized, MenuRootField, menu.gameObject);
            SetReference(serialized, PauseButtonField, pause);
            SetReference(serialized, ResumeButtonField, resume);
            SetReference(serialized, ResetButtonField, reset);
            SetReference(serialized, MainMenuButtonField, mainMenu);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReference(SerializedObject serialized, string field, UnityEngine.Object value)
        {
            serialized.FindProperty(field).objectReferenceValue = value;
        }

        private static RectTransform CreateOverlayScreen(string name, Transform parent, string title)
        {
            RectTransform root = CreateStretched(name, parent);
            CreateImage(CreateStretched(BackgroundName, root), OverlayColor);

            CreateText(
                CreateAnchored(TitleName, root, Center, new Vector2(0f, TitleOffset), new Vector2(TextWidth, LineHeight)),
                title,
                TitleFontSize,
                TextAlignmentOptions.Center,
                Color.white);

            return root;
        }

        private static Button CreateMenuButton(RectTransform parent, string name, string text, int order)
        {
            float offsetY = FirstButtonOffset - (order * (MenuButtonHeight + MenuButtonSpacing));
            RectTransform rect = CreateAnchored(
                name,
                parent,
                Center,
                new Vector2(0f, offsetY),
                new Vector2(MenuButtonWidth, MenuButtonHeight));

            Button button = CreateButton(rect, MenuButtonColor);
            CreateText(CreateStretched(LabelName, rect), text, ButtonFontSize, TextAlignmentOptions.Center, MenuButtonTextColor);
            return button;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static RectTransform CreateStretched(string name, Transform parent)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static RectTransform CreateAnchored(string name, Transform parent, Vector2 anchor, Vector2 offset, Vector2 size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
            return rect;
        }

        private static Image CreateImage(RectTransform rect, Color color)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Button CreateButton(RectTransform rect, Color color)
        {
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = CreateImage(rect, color);
            return button;
        }

        private static TMP_Text CreateText(RectTransform rect, string content, float fontSize, TextAlignmentOptions alignment, Color color)
        {
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            if (s_Font != null)
            {
                text.font = s_Font;
            }

            return text;
        }
    }
}
