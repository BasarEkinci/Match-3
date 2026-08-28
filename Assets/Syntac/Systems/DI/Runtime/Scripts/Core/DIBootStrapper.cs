using System;
using Syntac.DI.Core.Scopes;
using UnityEngine;

namespace Syntac.DI.Core
{
    /// <summary>
    /// Creates the application-wide <see cref="ProjectLifetimeScope"/> before the first scene loads,
    /// so the root scope exists no matter which scene Play was pressed from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Resources.Load{T}(string)"/> is synchronous and runs before the first scene: keep the
    /// prefab lightweight, every asset it references is loaded on the main thread at that point.
    /// </para>
    /// <para>
    /// The resource path is a literal here, so the prefab must never be moved or renamed.
    /// </para>
    /// <para>
    /// <see cref="s_Initialized"/> is reset by the domain reload, which Enter Play Mode Options must
    /// therefore keep enabled; without it a second Play session would skip the bootstrap entirely.
    /// </para>
    /// </remarks>
    internal static class DIBootStrapper
    {
        private const string k_ProjectLifetimeScopePath = "DI/ProjectLifetimeScope";

        private static bool s_Initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (s_Initialized)
            {
                return;
            }

            ProjectLifetimeScope prefab = Resources.Load<ProjectLifetimeScope>(k_ProjectLifetimeScopePath);
            if (prefab == null)
            {
                throw new NullReferenceException(
                    $"No {nameof(ProjectLifetimeScope)} prefab at Resources/{k_ProjectLifetimeScopePath}. " +
                    "The path is hard coded in DIBootStrapper; the prefab must not be moved or renamed.");
            }

            ProjectLifetimeScope scope = UnityEngine.Object.Instantiate(prefab);
            scope.gameObject.name = $"[{nameof(ProjectLifetimeScope)}]";
            scope.gameObject.hideFlags = HideFlags.NotEditable;
            UnityEngine.Object.DontDestroyOnLoad(scope.gameObject);

            s_Initialized = true;
        }
    }
}
