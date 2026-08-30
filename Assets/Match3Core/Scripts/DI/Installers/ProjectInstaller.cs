using System.Collections.Generic;
using Match3.Core.DI.Installers;
using Match3.Core.MessagePipe.Installers;
using VContainer;
using VContainer.Unity;

namespace Match3.Core.DI.Installers
{
    /// <summary>
    /// The only place framework modules are composed into the root scope. Adding a system is a single
    /// <c>yield return</c> in <see cref="FetchInstallers"/>.
    /// </summary>
    /// <remarks>
    /// Order does not affect resolution — VContainer builds the graph after every registration — but
    /// entry-point <c>Start()</c> order follows registration order, so pipes stay last.
    /// </remarks>
    public class ProjectInstaller : MonoInstaller
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
            yield return new ProjectPipeInstaller();
        }
    }
}
