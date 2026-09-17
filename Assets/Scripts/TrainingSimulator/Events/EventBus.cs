using System;
using System.Collections.Generic;

namespace TrainingSimulator.Events
{
    public sealed class EventBus : IEventBus
    {
        // На каждый тип сигнала — один многоадресный делегат.
        private readonly Dictionary<Type, Delegate> _handlers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            var type = typeof(T);
            _handlers[type] = _handlers.TryGetValue(type, out var existing)
                ? Delegate.Combine(existing, handler)
                : handler;
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var existing)) return;

            var result = Delegate.Remove(existing, handler);
            if (result == null) _handlers.Remove(type);
            else _handlers[type] = result;
        }

        public void Publish<T>(T signal)
        {
            if (_handlers.TryGetValue(typeof(T), out var existing))
                (existing as Action<T>)?.Invoke(signal);
        }
    }
}
