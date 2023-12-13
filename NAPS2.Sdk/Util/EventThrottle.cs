using System.Diagnostics.CodeAnalysis;

namespace NAPS2.Util;

internal class EventThrottle<T>
{
    private readonly Action<T> _eventCallback;

    private bool _hasLastValue;
    [MaybeNull]
    private T _lastValue;

    public EventThrottle(Action<T> eventCallback)
    {
        _eventCallback = eventCallback;
    }

    public void Reset()
    {
        _hasLastValue = false;
    }

    public void OnlyIfChanged(T value)
    {
        if (!_hasLastValue)
        {
            _lastValue = value;
            _hasLastValue = true;
            _eventCallback(value);
            return;
        }
        if (!Equals(value, _lastValue))
        {
            _lastValue = value;
            _eventCallback(value);
        }
    }
}