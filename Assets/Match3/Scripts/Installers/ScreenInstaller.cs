using Match3.Controller;
using Match3.Core.DI.Installers;
using VContainer;

namespace Match3.Installers
{
    public sealed class ScreenInstaller : MonoInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ScreenFlowController>(Lifetime.Singleton);
            builder.RegisterBuildCallback(container => container.Resolve<ScreenFlowController>());
        }
    }
}
