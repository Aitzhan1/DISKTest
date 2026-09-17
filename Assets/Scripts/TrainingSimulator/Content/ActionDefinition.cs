using System;
using TrainingSimulator.Interaction;
using UnityEngine;

namespace TrainingSimulator.Content
{
    // Одно ожидаемое действие: что сделать и с каким объектом сцены.
    // Ссылок на сцену не держит — объект ищется по id в рантайме.
    [Serializable]
    public sealed class ActionDefinition
    {
        [SerializeField, Tooltip("Текст для чек-листа, напр. «Взять паспорт».")]
        private string description;

        [SerializeField] private ExpectedActionType expectedType;

        [SerializeField, Tooltip("Id объекта сцены — поле Id соответствующего Interactable.")]
        private string targetId;

        public string Description => description;
        public ExpectedActionType ExpectedType => expectedType;
        public string TargetId => targetId;
    }
}
