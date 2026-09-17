using System.Collections.Generic;
using UnityEngine;

namespace TrainingSimulator.Content
{
    // Сценарий целиком: группы проходятся по порядку списка.
    // Количество групп и шагов не ограничено, текущий контент — 3 на 3.
    [CreateAssetMenu(fileName = "Scenario", menuName = "Training Simulator/Scenario", order = 0)]
    public sealed class ScenarioDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Сценарий тренировки";
        [SerializeField] private List<StepGroupDefinition> groups = new();

        public string DisplayName => displayName;
        public IReadOnlyList<StepGroupDefinition> Groups => groups;
    }
}
