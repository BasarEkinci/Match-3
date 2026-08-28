namespace Syntac.Signals
{
    /// <summary>
    /// Marker interface for all events published on a Syntac event bus.
    /// Implement this on <c>readonly struct</c> types: the bus passes events by <c>ref</c>,
    /// so struct signals are allocation-free.
    /// </summary>
    public interface ISignal
    {
    }
}
