using Match3.Model;
using Match3.Model.Enums;
using Match3.Signals;
using Match3.Core.MessagePipe.Pipes;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Match3.View
{
    public sealed class BoardInputView : MonoBehaviour
    {
        private const float DragThresholdRatio = 0.35f;

        private GamePipe m_GamePipe;
        private BoardGeometry m_Geometry;
        private Camera m_Camera;
        private GridPosition m_Origin;
        private Vector3 m_PressWorldPoint;
        private bool m_HasOrigin;

        [Inject]
        public void Construct(GamePipe gamePipe, BoardGeometry geometry)
        {
            m_GamePipe = gamePipe;
            m_Geometry = geometry;
        }

        private void Awake()
        {
            m_Camera = Camera.main;
        }

        private void Update()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null || m_Camera == null)
            {
                return;
            }

            if (pointer.press.wasPressedThisFrame)
            {
                BeginDrag(pointer);
                return;
            }

            if (!m_HasOrigin)
            {
                return;
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                m_HasOrigin = false;
                m_GamePipe.Raise(new TileTapSignal(m_Origin));
                return;
            }

            TryEmitDrag(pointer);
        }

        private void BeginDrag(Pointer pointer)
        {
            m_PressWorldPoint = ToWorldPoint(pointer);
            m_HasOrigin = m_Geometry.TryToGrid(m_PressWorldPoint, out m_Origin);
        }

        private void TryEmitDrag(Pointer pointer)
        {
            Vector3 delta = ToWorldPoint(pointer) - m_PressWorldPoint;
            if (delta.magnitude < DragThresholdRatio * m_Geometry.CellSize)
            {
                return;
            }

            m_HasOrigin = false;
            m_GamePipe.Raise(new TileDragSignal(m_Origin, ToDirection(delta)));
        }

        private Vector3 ToWorldPoint(Pointer pointer)
        {
            Vector2 screenPoint = pointer.position.ReadValue();
            return m_Camera.ScreenToWorldPoint(
                new Vector3(screenPoint.x, screenPoint.y, -m_Camera.transform.position.z));
        }

        private static GridDirection ToDirection(Vector3 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0f ? GridDirection.Right : GridDirection.Left;
            }

            return delta.y > 0f ? GridDirection.Up : GridDirection.Down;
        }
    }
}
