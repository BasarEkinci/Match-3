using Syntac.DI.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Syntac.DI.Core.Scopes
{
    /// <summary>
    /// Child scope of <see cref="ProjectLifetimeScope"/>, torn down with its scene.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The injection sweep in <c>Awake</c> is O(every component in the scene) and reflects over each one.
    /// Once the scene grows large, replace it with an explicit registration list.
    /// </para>
    /// <para>
    /// Only objects that exist when <c>Awake</c> runs are injected. Anything spawned later must go through
    /// <c>IObjectResolver.Instantiate(prefab)</c> or <c>IObjectResolver.Inject(instance)</c>.
    /// </para>
    /// </remarks>
    public class SceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private MonoInstaller[] m_MonoInstallers;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            foreach (MonoInstaller installer in m_MonoInstallers)
            {
                installer.Install(builder);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                Container.Inject(behaviour);
            }
        }
    }
}
