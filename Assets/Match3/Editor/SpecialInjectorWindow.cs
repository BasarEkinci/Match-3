using Match3.Model.Enums;
using Match3.Signals;
using Match3.Core.DI.Scopes;
using Match3.Core.MessagePipe.Pipes;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace Match3.Editor
{
    public sealed class SpecialInjectorWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Match3/Special Injector";
        private const string WindowTitle = "Special Injector";
        private const string SpecialLabel = "Special";
        private const string PlaceLabel = "Place On Random Gem";
        private const string PlayModeHint = "Enter play mode and start a round.";
        private const string MissingScopeHint = "Scene lifetime scope not found.";

        private SpecialTileType m_Special = SpecialTileType.Bomb;

        [MenuItem(MenuPath)]
        private static void Open() => GetWindow<SpecialInjectorWindow>(WindowTitle);

        private void OnGUI()
        {
            m_Special = (SpecialTileType)EditorGUILayout.EnumPopup(SpecialLabel, m_Special);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(PlayModeHint, MessageType.Info);
                return;
            }

            if (GUILayout.Button(PlaceLabel))
            {
                Place();
            }
        }

        private void Place()
        {
            SceneLifetimeScope scope = FindAnyObjectByType<SceneLifetimeScope>();
            if (scope == null)
            {
                Debug.LogWarning(MissingScopeHint);
                return;
            }

            scope.Container.Resolve<GamePipe>().Raise(new DebugSpecialRequestedSignal(m_Special));
        }
    }
}
