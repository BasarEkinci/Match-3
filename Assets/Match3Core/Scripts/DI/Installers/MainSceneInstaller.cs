using System.Collections.Generic;
using Match3.Installers;
using Match3.Core.DI.Installers;
using Match3.Core.MessagePipe.Installers;
using VContainer;
using VContainer.Unity;

namespace Match3.Core.DI.Installers
{
    public class MainSceneInstaller : MonoInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            foreach (IInstaller installer in FetchInstallers())
            {
                installer.Install(builder);
            }
        }

        private IEnumerable<IInstaller> FetchInstallers()
        {
            yield return new GamePipeInstaller();
            yield return new Match3ModelInstaller();
        }
    }
}
