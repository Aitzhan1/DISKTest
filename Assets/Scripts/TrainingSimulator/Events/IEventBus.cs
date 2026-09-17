using System;

namespace TrainingSimulator.Events
{
    // Типизированная шина publish/subscribe. Системы общаются сигналами и не знают друг о друге.
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Publish<T>(T signal);
    }
}
