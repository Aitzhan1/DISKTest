using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrainingSimulator.Content
{
    // Шаг сценария. Действий может быть несколько («подойти в зону И нажать кнопку»),
    // шаг закрывается, когда выполнены все.
    [Serializable]
    public sealed class StepDefinition
    {
        [SerializeField, Tooltip("Краткое описание для чек-листа и итогов.")]
        private string description;

        [SerializeField, Tooltip("Задел на будущее: сейчас порядок действий внутри шага не проверяется.")]
        private bool orderedActions;

        [SerializeField] private List<ActionDefinition> actions = new();

        public string Description => description;
        public bool OrderedActions => orderedActions;
        public IReadOnlyList<ActionDefinition> Actions => actions;
    }
}
