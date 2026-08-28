using GenericEventBus;
using JetBrains.Annotations;
using Syntac.Signals;

namespace Syntac.MessagePipe.Pipes
{
    /// <summary>
    /// Root-scope bus. Lives for the whole application: settings changed, language changed,
    /// online-service ready.
    /// </summary>
    /// <remarks>
    /// Lifetime: the event-bus listener tables are static dictionaries keyed by bus instance, so a bus
    /// that is never <c>Dispose</c>d leaks its listener table. VContainer disposes <c>IDisposable</c>
    /// singletons with the scope, which is why pipes must be registered as singletons in a scope and
    /// never constructed with a free-floating <c>new</c>.
    /// </remarks>
    [UsedImplicitly]
    public class ProjectPipe : GenericEventBus<ISignal>
    {
    }
}
