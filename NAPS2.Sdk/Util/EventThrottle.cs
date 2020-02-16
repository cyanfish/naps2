using System;
using System.Diagnostics.CodeAnalysis;

namespace NAPS2.Util
{
    public class EventThrottle<T>
    {
        private readonly Action<T> eventCallback;

        private bool hasLastValue;
        [MaybeNull]
        private T lastValue;

        public EventThrottle(Action<T> eventCallback)
        {
            this.eventCallback = eventCallback;
        }

        public void OnlyIfChanged(T value)
        {
            if (!hasLastValue)
            {
                lastValue = value;
                hasLastValue = true;
                eventCallback(value);
                return;
            }
            if (!Equals(value, lastValue))
            {
                lastValue = value;
                eventCallback(value);
            }
        }
    }
}