using Match3.Core.MessagePipe.Pipes;
using VContainer;
using VContainer.Unity;

namespace Match3.Core.MessagePipe.Installers
{
    public class ProjectPipeInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<ProjectPipe>(Lifetime.Singleton);
        }
    }
}
