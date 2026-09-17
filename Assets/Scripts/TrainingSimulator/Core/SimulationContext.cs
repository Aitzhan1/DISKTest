using TrainingSimulator.Events;
using UnityEngine;

namespace TrainingSimulator.Core
{
    // Точка доступа к шине в пределах сцены. Один такой объект на сцену тренировки.
    //
    // Инициализация в OnEnable, а не в Awake: если скрипты меняются во время Play Mode,
    // Unity перезагружает сборку, статика обнуляется, а Awake больше не вызывается.
    // Поэтому же Current умеет найти объект в сцене заново, а шина создаётся лениво.
    [DefaultExecutionOrder(-100)]
    public sealed class SimulationContext : MonoBehaviour
    {
        private static SimulationContext _current;

        public static SimulationContext Current
        {
            get
            {
                if (!_current) _current = FindFirstObjectByType<SimulationContext>();
                return _current;
            }
        }

        private IEventBus _bus;

        public IEventBus Bus => _bus ??= new EventBus();

        private void OnEnable()
        {
            if (_current && _current != this)
            {
                Destroy(gameObject);
                return;
            }
            _current = this;
        }

        private void OnDisable()
        {
            if (_current == this) _current = null;
        }
    }
}
