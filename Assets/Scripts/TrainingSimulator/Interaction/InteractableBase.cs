using TrainingSimulator.Core;
using TrainingSimulator.Events;
using UnityEngine;

namespace TrainingSimulator.Interaction
{
    // Общее для всех интерактивных объектов: id, регистрация в реестре и отправка
    // сигнала в шину. Наследники лишь вызывают Report, когда сработало их событие.
    // Верно это или нет, решает ScenarioRunner.
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField, Tooltip("Стабильный id, по нему объект находит сценарий. " +
                                 "Если пусто — берётся имя объекта.")]
        private string id;

        public string Id => string.IsNullOrEmpty(id) ? name : id;
        public abstract ExpectedActionType ActionType { get; }
        public GameObject GameObject => gameObject;

        protected virtual void OnEnable() => InteractableRegistry.Register(this);
        protected virtual void OnDisable() => InteractableRegistry.Unregister(this);

        protected void Report()
        {
            var bus = SimulationContext.Current?.Bus;
            if (bus == null)
            {
                // Иначе потерянное действие выглядит как ошибка игрока.
                Debug.LogWarning($"[{name}] Нет SimulationContext — взаимодействие «{Id}» потеряно.", this);
                return;
            }
            bus.Publish(new InteractionPerformedSignal(this));
        }
    }
}
