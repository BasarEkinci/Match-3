using System.Collections.Generic;
using Syntac.DI.Core.Installers;
using Syntac.MessagePipe.Installers;
using VContainer;
using VContainer.Unity;

namespace Syntac.DI.Installers
{
    /// <summary>
    /// Composes the framework modules that live and die with <c>Main.unity</c>.
    /// </summary>
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
        }
    }
}
