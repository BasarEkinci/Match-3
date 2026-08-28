using Syntac.DI.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Syntac.DI.Core.Scopes
{
    /// <summary>
    /// Root scope of the application, instantiated by <see cref="DIBootStrapper"/> and kept alive across
    /// scene loads. Holds everything whose lifetime is the process, not the scene.
    /// </summary>
    /// <remarks>
    /// <see cref="s_Initialized"/> guards against a second registration pass over the same static state
    /// and is cleared by the domain reload, so Enter Play Mode Options must keep Reload Domain enabled.
    /// If domain reload is ever disabled, add a
    /// <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c> reset for
    /// this flag and for <see cref="DIBootStrapper"/>'s.
    /// </remarks>
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private MonoInstaller[] m_Installers;

        private static bool s_Initialized;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            if (s_Initialized)
            {
                return;
            }

            s_Initialized = true;

            foreach (MonoInstaller installer in m_Installers)
            {
                installer.Install(builder);
            }
        }
    }
}
