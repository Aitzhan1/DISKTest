using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrainingSimulator.Content
{
    // Блок связанных шагов, например «Проверка документов».
    // Шаги внутри группы линейны: нарушение порядка закрывает группу целиком.
    [Serializable]
    public sealed class StepGroupDefinition
    {
        [SerializeField] private string title;

        [SerializeField, TextArea(2, 5),
         Tooltip("Сообщение о порядке действий. Показывается при активации группы.")]
        private string briefing;

        [SerializeField] private List<StepDefinition> steps = new();

        public string Title => title;
        public string Briefing => briefing;
        public IReadOnlyList<StepDefinition> Steps => steps;
    }
}
