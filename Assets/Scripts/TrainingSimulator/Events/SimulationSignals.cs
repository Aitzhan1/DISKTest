using System.Collections.Generic;
using TrainingSimulator.Interaction;
using TrainingSimulator.Runtime;

namespace TrainingSimulator.Events
{
    // Игрок провзаимодействовал с объектом — вход для ScenarioRunner.
    public readonly struct InteractionPerformedSignal
    {
        public readonly IInteractable Source;
        public InteractionPerformedSignal(IInteractable source) => Source = source;
    }

    // Началась новая группа: показать брифинг и чек-лист.
    public readonly struct GroupStartedSignal
    {
        public readonly int GroupIndex;
        public readonly int GroupCount;
        public readonly string Title;
        public readonly string Briefing;
        public readonly IReadOnlyList<string> StepDescriptions;

        public GroupStartedSignal(int groupIndex, int groupCount, string title, string briefing,
            IReadOnlyList<string> stepDescriptions)
        {
            GroupIndex = groupIndex;
            GroupCount = groupCount;
            Title = title;
            Briefing = briefing;
            StepDescriptions = stepDescriptions;
        }
    }

    // Шаг сменил статус: обновить отметку в чек-листе.
    public readonly struct StepStatusChangedSignal
    {
        public readonly int GroupIndex;
        public readonly int StepIndex;
        public readonly StepStatus Status;

        public StepStatusChangedSignal(int groupIndex, int stepIndex, StepStatus status)
        {
            GroupIndex = groupIndex;
            StepIndex = stepIndex;
            Status = status;
        }
    }

    // Сценарий пройден: показать итоги.
    public readonly struct ScenarioCompletedSignal
    {
        public readonly ScenarioResult Result;
        public ScenarioCompletedSignal(ScenarioResult result) => Result = result;
    }
}
