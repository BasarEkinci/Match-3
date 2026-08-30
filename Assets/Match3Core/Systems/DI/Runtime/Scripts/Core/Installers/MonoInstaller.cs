using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Match3.Core.DI.Installers
{
    /// <summary>
    /// Base class for installers that need to carry asset references, which a plain
    /// <see cref="IInstaller"/> cannot: only a component can be serialized into a prefab or a scene.
    /// </summary>
    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
