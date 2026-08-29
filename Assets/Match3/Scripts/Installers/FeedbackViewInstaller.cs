using Match3.View;
using Syntac.DI.Core.Installers;
using UnityEngine;
using VContainer;

namespace Match3.Installers
{
    public sealed class FeedbackViewInstaller : MonoInstaller
    {
        [SerializeField] private ParticleSystem burstPrefab;
        [SerializeField] private AudioClip matchClip;

        public override void Install(IContainerBuilder builder)
        {
            builder.Register<BoardFeedbackView>(Lifetime.Singleton)
                .WithParameter(burstPrefab)
                .WithParameter(matchClip);
            builder.RegisterBuildCallback(container => container.Resolve<BoardFeedbackView>());
        }
    }
}
