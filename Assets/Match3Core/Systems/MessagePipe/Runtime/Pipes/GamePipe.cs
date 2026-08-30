using GenericEventBus;
using JetBrains.Annotations;
using Match3.Core.Signals;

namespace Match3.Core.MessagePipe.Pipes
{
    /// <summary>
    /// Scene-scope bus: damage, pickup, death, UI. Everything that must die when the scene is
    /// reloaded belongs here; everything that must outlive the scene belongs on <see cref="ProjectPipe"/>.
    /// </summary>
    /// <remarks>
    /// Lifetime: the event-bus listener tables are static dictionaries keyed by bus instance, so a bus
    /// that is never <c>Dispose</c>d leaks its listener table. VContainer disposes <c>IDisposable</c>
    /// singletons with the scope, which is why pipes must be registered as singletons in a scope and
    /// never constructed with a free-floating <c>new</c>.
    /// </remarks>
    [UsedImplicitly]
    public class GamePipe : GenericEventBus<ISignal>
    {
    }
}
