using System;
using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;
using Match3.Model;
using Match3.Signals;
using Match3.Core.MessagePipe.Pipes;
using UnityEngine;
using UnityEngine.Audio;

namespace Match3.View
{
    public sealed class BoardFeedbackView : IDisposable
    {
        private const string RootName = "Feedback";
        private const int BurstParticleCount = 12;
        private const float BasePitch = 1f;
        private const float PitchPerCascadeStep = 0.08f;
        private const float MaxPitch = 2f;
        private const float FirstCascadeStep = 1f;
        private const float ShakeStrength = 0.12f;
        private const float ShakeDuration = 0.18f;
        private const int ShakeFrequency = 12;

        private readonly GamePipe m_GamePipe;
        private readonly BoardGeometry m_Geometry;
        private readonly Transform m_Root;
        private readonly ParticleSystem m_Burst;
        private readonly AudioSource m_AudioSource;
        private readonly Camera m_Camera;

        private MotionHandle m_ShakeHandle;
        private bool m_IsDisposed;
        
        public BoardFeedbackView(GamePipe gamePipe, BoardGeometry geometry, ParticleSystem burstPrefab, AudioResource matchContainer)
        {
            m_GamePipe = gamePipe;
            m_Geometry = geometry;
            m_Camera = Camera.main;
            m_Root = new GameObject(RootName).transform;
            m_AudioSource = m_Root.gameObject.AddComponent<AudioSource>();
            m_AudioSource.playOnAwake = false;
            m_AudioSource.resource = matchContainer;
            m_Burst = CreateBurst(burstPrefab);

            m_GamePipe.SubscribeTo<CellsClearedSignal>(OnCellsCleared);
        }

        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            m_ShakeHandle.TryCancel();
            m_GamePipe.UnsubscribeFrom<CellsClearedSignal>(OnCellsCleared);
            if (m_Root != null)
            {
                UnityEngine.Object.Destroy(m_Root.gameObject);
            }
        }

        private ParticleSystem CreateBurst(ParticleSystem prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            return UnityEngine.Object.Instantiate(prefab, m_Root);
        }

        private void OnCellsCleared(ref CellsClearedSignal signal)
        {
            EmitBursts(signal.Cells);
            PlayMatchSound(signal.CascadeStep);
            ShakeCamera();
        }

        private void EmitBursts(IReadOnlyList<ClearedCell> cells)
        {
            if (m_Burst == null)
            {
                return;
            }

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            for (int index = 0; index < cells.Count; index++)
            {
                emitParams.position = m_Geometry.ToWorld(cells[index].Position);
                emitParams.applyShapeToPosition = true;
                m_Burst.Emit(emitParams, BurstParticleCount);
            }
        }

        private void PlayMatchSound(int cascadeStep)
        {
            if (m_AudioSource.resource == null)
            {
                return;
            }

            m_AudioSource.pitch = Mathf.Min(
                BasePitch + ((cascadeStep - FirstCascadeStep) * PitchPerCascadeStep),
                MaxPitch);
            m_AudioSource.Play();
        }

        private void ShakeCamera()
        {
            if (m_Camera == null)
            {
                return;
            }

            m_ShakeHandle.TryCancel();
            m_ShakeHandle = LMotion.Shake
                .Create(m_Camera.transform.position, Vector3.one * ShakeStrength, ShakeDuration)
                .WithFrequency(ShakeFrequency)
                .BindToPosition(m_Camera.transform);
        }
    }
}
