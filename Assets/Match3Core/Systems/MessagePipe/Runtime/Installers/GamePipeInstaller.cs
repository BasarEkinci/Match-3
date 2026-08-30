using Match3.Core.MessagePipe.Pipes;
using VContainer;
using VContainer.Unity;

namespace Match3.Core.MessagePipe.Installers
{
    public class GamePipeInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<GamePipe>(Lifetime.Singleton);
        }
    }
}
