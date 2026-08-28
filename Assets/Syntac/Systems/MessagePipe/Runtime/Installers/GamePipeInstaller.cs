using Syntac.MessagePipe.Pipes;
using VContainer;
using VContainer.Unity;

namespace Syntac.MessagePipe.Installers
{
    public class GamePipeInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<GamePipe>(Lifetime.Singleton);
        }
    }
}
