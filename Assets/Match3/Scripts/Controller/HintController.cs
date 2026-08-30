using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Match3.Model;
using Match3.Model.Matching;
using Match3.Model.Settings;
using Match3.Signals;
using Match3.Core.MessagePipe.Pipes;

namespace Match3.Controller
{
    public sealed class HintController : IDisposable
    {
        private readonly GamePipe m_GamePipe;
        private readonly IMoveScanner m_MoveScanner;
        private readonly IHintSettings m_Settings;
        private readonly CancellationTokenSource m_Lifetime = new CancellationTokenSource();

        private Board m_Board;
        private CancellationTokenSource m_Idle;
        private bool m_IsDisposed;

        public HintController(GamePipe gamePipe, IMoveScanner moveScanner, IHintSettings settings)
        {
            m_GamePipe = gamePipe;
            m_MoveScanner = moveScanner;
            m_Settings = settings;

            m_GamePipe.SubscribeTo<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.SubscribeTo<InputLockChangedSignal>(OnInputLockChanged);
            m_GamePipe.SubscribeTo<TileDragSignal>(OnTileDragged);
            m_GamePipe.SubscribeTo<TileTapSignal>(OnTileTapped);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            StopIdleTimer();
            m_Lifetime.Cancel();
            m_Lifetime.Dispose();
            m_GamePipe.UnsubscribeFrom<BoardCreatedSignal>(OnBoardCreated);
            m_GamePipe.UnsubscribeFrom<InputLockChangedSignal>(OnInputLockChanged);
            m_GamePipe.UnsubscribeFrom<TileDragSignal>(OnTileDragged);
            m_GamePipe.UnsubscribeFrom<TileTapSignal>(OnTileTapped);
        }

        private void OnBoardCreated(ref BoardCreatedSignal signal)
        {
            m_Board = signal.Board;
        }

        private void OnInputLockChanged(ref InputLockChangedSignal signal)
        {
            StopIdleTimer();
            if (!signal.IsLocked)
            {
                StartIdleTimer();
            }
        }

        private void OnTileDragged(ref TileDragSignal signal) => RestartIdleTimer();

        private void OnTileTapped(ref TileTapSignal signal) => RestartIdleTimer();

        private void RestartIdleTimer()
        {
            if (m_Idle == null)
            {
                return;
            }

            StopIdleTimer();
            StartIdleTimer();
        }

        private void StartIdleTimer()
        {
            m_Idle = CancellationTokenSource.CreateLinkedTokenSource(m_Lifetime.Token);
            RunIdleTimer(m_Idle.Token).Forget();
        }

        private void StopIdleTimer()
        {
            if (m_Idle == null)
            {
                return;
            }

            m_Idle.Cancel();
            m_Idle.Dispose();
            m_Idle = null;
        }

        private async UniTaskVoid RunIdleTimer(CancellationToken token)
        {
            float delay = m_Settings.IdleSeconds;
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

                if (m_Board != null && m_MoveScanner.TryFindMove(m_Board, out GridPosition from, out GridPosition to))
                {
                    m_GamePipe.Raise(new HintShownSignal(from, to));
                }

                delay = m_Settings.RepeatSeconds;
            }
        }
    }
}
