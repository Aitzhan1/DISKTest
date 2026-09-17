using UnityEngine;

namespace TrainingSimulator.Interaction
{
    // Интерактивный объект сцены. Сценарий ссылается на него по Id, а не по типу
    // компонента, поэтому данные остаются независимыми от конкретной сцены.
    public interface IInteractable
    {
        string Id { get; }
        ExpectedActionType ActionType { get; }

        // Цель для подсветки.
        GameObject GameObject { get; }
    }
}
