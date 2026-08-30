using Match3.Core.MessagePipe.Pipes;
using VContainer;

namespace Match3.Core.DI.Installers
{
    public sealed class ProjectInstaller : MonoInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ProjectPipe>(Lifetime.Singleton);
        }
    }
}
