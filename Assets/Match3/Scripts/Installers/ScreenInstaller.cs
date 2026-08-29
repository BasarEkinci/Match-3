using Match3.Controller;
using Match3.View;
using Syntac.DI.Core.Installers;
using TMPro;
using UnityEngine;
using VContainer;

namespace Match3.Installers
{
    public sealed class ScreenInstaller : MonoInstaller
    {
        [SerializeField] private TMP_FontAsset font;

        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ScreenFlowController>(Lifetime.Singleton);
            builder.Register<MainScreenView>(Lifetime.Singleton).WithParameter(font);
            builder.Register<PauseScreenView>(Lifetime.Singleton).WithParameter(font);
            builder.Register<RoundEndView>(Lifetime.Singleton).WithParameter(font);
            builder.RegisterBuildCallback(container => container.Resolve<ScreenFlowController>());
            builder.RegisterBuildCallback(container => container.Resolve<MainScreenView>());
            builder.RegisterBuildCallback(container => container.Resolve<PauseScreenView>());
            builder.RegisterBuildCallback(container => container.Resolve<RoundEndView>());
        }
    }
}
