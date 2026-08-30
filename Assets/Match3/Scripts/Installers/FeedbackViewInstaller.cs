using Match3.View;
using Match3.Core.DI.Installers;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace Match3.Installers
{
    public sealed class FeedbackViewInstaller : MonoInstaller
    {
        [SerializeField] private ParticleSystem burstPrefab;
        [SerializeField] private AudioResource matchContainer;

        public override void Install(IContainerBuilder builder)
        {
            builder.Register<BoardFeedbackView>(Lifetime.Singleton)
                .WithParameter(burstPrefab)
                .WithParameter(matchContainer);
            builder.RegisterBuildCallback(container => container.Resolve<BoardFeedbackView>());
        }
    }
}
