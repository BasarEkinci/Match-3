using System;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Signals;
using Syntac.MessagePipe.Pipes;

namespace Match3.Controller
{
    public sealed class InputController : IDisposable
    {
        private const int Step = 1;

        private readonly GamePipe m_GamePipe;

        private bool m_IsLocked;
        private bool m_IsDisposed;

        public InputController(GamePipe gamePipe)
        {
            m_GamePipe = gamePipe;
            m_GamePipe.SubscribeTo<TileDragSignal>(OnTileDragged);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_GamePipe.UnsubscribeFrom<TileDragSignal>(OnTileDragged);
            m_GamePipe.UnsubscribeFrom<InputLockChangedSignal>(OnInputLockChanged);
        }

        private void OnInputLockChanged(ref InputLockChangedSignal signal)
        {
            m_IsLocked = signal.IsLocked;
        }

        private void OnTileDragged(ref TileDragSignal signal)
        {
            if (m_IsLocked)
            {
                return;
            }

            m_GamePipe.Raise(new SwapRequestedSignal(signal.Origin, Neighbour(signal.Origin, signal.Direction)));
        }

        private static GridPosition Neighbour(GridPosition origin, GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up:
                    return new GridPosition(origin.X, origin.Y + Step);
                case GridDirection.Down:
                    return new GridPosition(origin.X, origin.Y - Step);
                case GridDirection.Left:
                    return new GridPosition(origin.X - Step, origin.Y);
                default:
                    return new GridPosition(origin.X + Step, origin.Y);
            }
        }
    }
}
